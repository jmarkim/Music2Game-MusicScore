using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicXml;
using MidiSharp;
using System.IO;
using System.IO.Compression;

//PRBL :: Resolver partituras múltiplas (i.e. pianos com uma clave para cada mão)
//SLTN :: Ignorar pentagrama incorreto, loop for (1 a n) << possível super-adaptação >>

namespace MusicScore {
    public static class ScoreBuilder {

        internal class MIDIBuilder {
            // Singleton para armazenar informações sobre o processo

            // Constante para menor elemento considerado (Fração de semínima)
            internal const int MINIMUN_TEMPO = 32;

            // Objeto em construção
            private Score _score;
            // Instrumento em construção
            private Part _part;
            // Compasso em construção
            private Measure _measure;
            // Posição de elemento no compasso;
            private int _position;
            // Tônica do compasso
            private Pitches _tonic;
            // Lista de notas de acorde
            private List<MeasureElement> _chordNotes;

            private int errC;

            // Contagem de tempo (clicks)
            private long _time;
            public long TimeElapsed {
                get { return _time; }
                set { _time = value; }
            }

            // Registro de tempo do último evento
            private long _last;
            public long TimeLastEvent {
                get { return _last; }
                set { _last = value; }
            }

            // Registro da fórmula de compasso atual
            private int _size;
            public int MeasureSize {
                get { return _size; }
                set { _size = value; }
            }

            // Registro da última fórmula de compasso
            private int _oldSize;
            public int LastMeasureSize {
                get { return _oldSize; }
                set { _oldSize = value; }
            }

            // Flag de primeiro instrumento
            private bool _first;
            public bool FirstPart {
                get { return _first; }
                set { _first = value; }
            }

            // Flag de acorde
            //private bool _chord;
            //public bool IsChord {
            //    get { return _chord; }
            //    set { _chord = value; }
            //}

            // Registra mudança de assinatura de tempo, usado a partir do segundo instrumento
            private List<Tuple<long, int>> _timeChanges;
            public List<Tuple<long, int>> TimeSignatureChanges {
                get { return _timeChanges; }
            }

            // Registra vozes ativas
            private List<Tuple<int, int, long>> _active;
            public List<Tuple<int, int, long>> ActiveVoices {
                get { return _active; }
            }


            // Construtor
            private MIDIBuilder() {
                Reset();
            }

            // Retorna às configurações iniciais
            public void Reset() {
                // Inicia objeto Score
                _score = new Score();
                _score.Parts = new List<Part>();

                _score.Parts.Clear();

                errC = 0;

                // Inicia variáveis auxiliares
                _position = 0;
                _time = 0;
                _last = 0;
                _size = -1;
                _oldSize = -1;
                _first = true;
                //_chord = false;

                // Inicia Lisas
                _active = new List<Tuple<int, int, long>>();
                _chordNotes = new List<MeasureElement>();
                _timeChanges = new List<Tuple<long, int>>();
            }

            // Instância
            private static MIDIBuilder builder;
            public static MIDIBuilder Instance {
                get {
                    if (builder == null) {
                        builder = new MIDIBuilder();
                    }
                    return builder;
                }
            }

            // Valor da semi-breve
            private int _whole;
            public int WholeNote {
                get { return _whole; }
                set { _whole = value; }
            }

            // Retorna diferença de tempo entre o acumulado e o último evento
            public int EventDeltaTime() {
                return ( int )(_time - _last);
            }

            // Atualiza registro de tempo para último evento
            public void SaveLastEvent() {
                _last = _time;
            }

            // Atualiza a fórmula do compasso
            public void ChangeTimeSignature(MidiSharp.Events.Meta.TimeSignatureMetaMidiEvent timeSig) {
                int numerator = timeSig.Numerator;
                int denominator = ( int )(Math.Pow(2, timeSig.Denominator));
                _oldSize = _size;
                _size = numerator * _whole / denominator;
                _timeChanges.Add(new Tuple<long, int>(_time, _size));
            }

            // Atualiza "assinatura de chave"
            public void ChangeKey(MidiSharp.Events.Meta.KeySignatureMetaMidiEvent keySig) {
                // Considera apenas primeira assinatura
                if (_tonic != Pitches.NA) {
                    return;
                }

                switch (keySig.Key) {
                    case Key.NoFlatsOrSharps:
                    case Key.Flat7:
                    case Key.Sharp7:
                        _tonic = Pitches.C;
                        break;

                    case Key.Sharp2:
                    case Key.Flat5:
                        _tonic = Pitches.D;
                        break;

                    case Key.Sharp4:
                    case Key.Flat3:
                        _tonic = Pitches.E;
                        break;

                    case Key.Sharp6:
                    case Key.Flat1:
                        _tonic = Pitches.F;
                        break;

                    case Key.Sharp1:
                    case Key.Flat6:
                        _tonic = Pitches.G;
                        break;

                    case Key.Sharp3:
                    case Key.Flat4:
                        _tonic = Pitches.A;
                        break;

                    case Key.Sharp5:
                    case Key.Flat2:
                        _tonic = Pitches.B;
                        break;

                    default:
                        _tonic = Pitches.C;
                        break;
                }
            }

            // Checa ocorrência de uma pausa
            public void CheckForRest(bool timeSigEvent = false) {
                if (EventDeltaTime() > _whole / MINIMUN_TEMPO && _active.Count == 0) {
                    AddRest(EventDeltaTime(), timeSigEvent);
                }
            }

            // Processa evento "onNote"
            public void OnNoteEvent(MidiSharp.Events.Voice.Note.OnNoteVoiceMidiEvent onNote) {
                if (onNote.Velocity == 0) {
                    // Desativa voz
                    OffVoice(new Tuple<int, int, long>(onNote.Channel, onNote.Note, _time));
                } else {
                    // Ativa voz
                    OnVoice(new Tuple<int, int, long>(onNote.Channel, onNote.Note, _time));
                }
            }

            // Processa evento "offNote"
            public void OffNoteEvent(MidiSharp.Events.Voice.Note.OffNoteVoiceMidiEvent offNote) {
                if (_tonic == Pitches.NA) {
                    Console.WriteLine("   !!! OffNoteEvent() -> Assinatura de chave indefinida !!!");
                }

                // Desativa voz
                OffVoice(new Tuple<int, int, long>(offNote.Channel, offNote.Note, _time));
            }

            // Adiciona pausa ao instrumento
            public void AddRest(int duration, bool timeSigEvnt) {
                //Console.WriteLine("errC: {0}", ++errC);

                AddNote( BuildElement(true, duration, ( int )_last), timeSigEvnt );
            }

            // Ativa voz
            public void OnVoice(Tuple<int,int,long> voice) {
                if (_size < 0) {
                    return; // Ignora notas  antes da definição da fórmula do compasso
                }

                if (_active.Count > 0) {
                    //_chord = true;
                    int delta = ( int )(voice.Item3 - _active.First().Item3);
                    if (delta < _whole / MINIMUN_TEMPO / 2) {
                        // Considera que as notas começam juntas
                        _active.Add(new Tuple<int, int, long>(voice.Item1, voice.Item2, _active.First().Item3));
                    } else {
                        // Delta significativo, Resolve "sub-acorde"
                        // Resolve pré-acode
                        EndChord(voice.Item3, true);
                        // Aadiciona ao acorde novo acorde
                        _active.Add(voice);
                    }
                } else {
                    _active.Add(voice);
                }
            }

            // Desativa voz e adiciona ao instrumento
            public void OffVoice(Tuple<int, int, long> voice) {

                // Checa existência da voz na lista de ativas
                Tuple<int, int, long> activationInstance = _active.Find(a => a.Item1 == voice.Item1 && a.Item2 == voice.Item2);

                // Checa se a voz foi encontrada
                if (activationInstance == null) {
                    return; // Ignora notas antes da definição da fórmula, ou que não foram ativadas
                }

                // Remove voz da lista de ativas
                //_active.Remove(activationInstance);

                // Calcula duração da nota
                int duration = ( int )(voice.Item3 - activationInstance.Item3);

                // Ignora notas de duração despresível
                if (duration >= _whole / MINIMUN_TEMPO) {

                    if (_active.Count > 1) {
                        // É acorde
                        EndChord(voice.Item3, false);
                        // Atualiza refência à instância de ativação. Possivelmente perdida quando Item3 foi atualizado em EndChod()
                        activationInstance = _active.Find(a => a.Item1 == voice.Item1 && a.Item2 == voice.Item2);

                    } else {
                        // Nota solo

                        // Adiciona elemento ao intrumento
                        AddNote(BuildElement(false, duration, ( int )activationInstance.Item3, voice.Item2));
                    }
                }

                    _active.Remove(activationInstance);
            }

            //private void AddElement(MeasureElement element, bool timeSigEvnt = false) {
            //    // Checa flag de acorde
            //    if (element.Type == MeasureElementType.Chord) {
            //        _chordNotes.Add(element);

            //    } else if (element.Type == MeasureElementType.ChordBase) {
            //        EndChord(element); // A ser implementado

            //    } else {
            //        AddNote(element, timeSigEvnt);
            //    }
            //}

            // Finaliza acordes, adicionando todas as notas ao compasso
            private void EndChord(long refTime, bool newNote) {
                if (_chordNotes.Count != 0) {
                    Console.WriteLine("   !!! EndChord() -> Falha em esaziar _chordNotes !!!");
                }

                List<Tuple<int, int, long>> rest = new List<Tuple<int, int, long>>();
                int duration = 0;
                // Finalizando acorde devido a introdução de nova nota
                foreach (var note in _active) {
                    // Calcula duração parcial da nota
                    duration = ( int )(refTime- note.Item3);
                    // Adiciona à lista acorde nota com duração calculada
                    _chordNotes.Add(BuildElement(false, duration, ( int )note.Item3, note.Item2));
                    // Atualiza tempo de ativação na lista _active
                    rest.Add(new Tuple<int, int, long>(note.Item1, note.Item2, refTime));
                }
                AddChord(duration);
                _active = rest;
            }

            // Adiciona acorde ao instrumento
            private void AddChord(int chordDuration) {
                
                // Atualiza, se necessário tamanho do compasso
                SetMeasureSize(false);

                // Calcula espaço livre no compasso
                int available = _measure.Size - _position;

                if (chordDuration > available) {
                    // Quebra notas do acorde para caber no compasso
                    List<MeasureElement> rest = new List<MeasureElement>(_chordNotes.Count); // Lista que armazena restante da duração
                    List<MeasureElement> fit = new List<MeasureElement>(_chordNotes.Count); // Lista que armazena a parte que cabe
                    foreach (var note in _chordNotes) {
                        // Clona nota
                        MeasureElement clone = new MeasureElement();
                        if (note.Note != null) {
                            int Octave = note.Note.Octave;
                            Pitches Pitch = note.Note.Pitch;
                            int Tone = note.Note.Tone;
                            Scale Role = note.Note.Role;
                            Note cloned = new Note(Pitch, Octave, Tone, Role);
                            clone.Note = cloned;
                        }
                        // Clone salva o excedente
                        clone.Duration = note.Duration - available;
                        rest.Add(clone);
                        // Note dura o espaço disponível
                        note.Duration = available;
                        fit.Add(note);

                        // Adiciona a parde que cabe
                        _chordNotes = fit;
                        AddChord(available);

                        // Adiciona recursivamente o excesso
                        _chordNotes = rest;
                        AddChord(chordDuration - available);

                    }

                } else {
                    // Adiciona notas no compasso
                    MeasureElement chordBase = _chordNotes.First(); // Define uma nota como base do acorde
                    chordBase.Type = MeasureElementType.ChordBase;
                    _chordNotes.RemoveAt(0); // Remove nota base da lista
                    foreach (var chordNote in _chordNotes) {
                        chordNote.Type = MeasureElementType.Chord; // Define notas na lista como notas de acorde
                        AddNote(chordNote); // Adiciona no compasso
                    }
                    AddNote(chordBase); // Adicionda base do acorde, que avançará _position
                }

                _chordNotes.RemoveAll(a => true);
            }

            // Adiciona, de fato, nota ao compasso
            private void AddNote(MeasureElement note, bool timeSigEvnt = false) {
                // Ignora nota de duração despresível
                if (note.Duration < _whole / MINIMUN_TEMPO) {
                    return;
                }

                // Atualiza, se necessário tamanho do compasso
                SetMeasureSize(timeSigEvnt);

                // Se o compasso ainda não tem tamanho definido, nota ocorreu antes da definição da fórmula
                if (_measure.Size < 0) {
                    return;
                }

                // Tamanho do compasso
                int size = _measure.Size;

                // Checa se a nota cabe no compasso
                int available =  _measure.Size - _position;
                if (note.Duration > available) {
                    // Quebra nota e inicia novo compasso
                    MeasureElement clone = new MeasureElement();
                    clone.Type = note.Type;
                    if (note.Note != null) {
                        int Octave = note.Note.Octave;
                        Pitches Pitch = note.Note.Pitch;
                        int Tone = note.Note.Tone;
                        Scale Role = note.Note.Role;
                        Note cloned = new Note(Pitch, Octave, Tone, Role);
                        clone.Note = cloned;
                    }
                    clone.Duration = note.Duration - available;
                    note.Duration = available;
                    AddNote(note);
                    note.Duration = size;

                    // Checa se o restante é significativo
                    while (clone.Duration > size) {
                        AddNote(note);
                        clone.Duration -= size;
                    }

                    if (clone.Duration > _whole / MINIMUN_TEMPO) {
                        // Valor significativo
                        AddNote(clone);
                    }

                } else {
                    // Adiciona nota
                    // Define Posição da nota
                    note.Position = _position;

                    // Define osição na escala
                    

                    // Atualiza marcador de posição
                    if (note.Type != MeasureElementType.Chord) {
                        _position += note.Duration;
                    }

                    // Adiciona ao Objeto compasso
                    _measure.AddNote(note);

                    // Inicia novo compasso se necessário
                    if (_measure.IsFull()) {
                        NewMeasure();
                    }
                }
            }

            // Define tamanho do compasso se necessário
            private void SetMeasureSize(bool timeSigEvnt) {
                // Checa necessidade
                if (_measure.Size < 0) {
                    // Necessário
                    if (_first) {
                        // Se primeiro elemento instrumento
                        if (timeSigEvnt) {
                            _measure.Size = _oldSize;
                        } else {
                            _measure.Size = _size;
                        }
                    } else {
                        // Checa lista de Mudanças
                        _measure.Size = _timeChanges.FindAll(a => _time > a.Item1).Last().Item2;
                    }
                }
            }

            // Constrói um elemento de compasso
            private MeasureElement BuildElement(bool isRest, int duration, int position, int midi = -1) {
                MeasureElement elmnt = new MeasureElement();

                // Define duração do elemento
                elmnt.Duration = duration;
                elmnt.Position = position;

                // Atualiza marcador de posição do compasso
                //_position += duration;
                
                // Define tipo de elemento
                if (isRest) {
                    elmnt.Type = MeasureElementType.Rest;
                } else {
                    elmnt.Type = MeasureElementType.Note;
                    // Para tipos diferente de "Rest", Define Objeto "Note"
                    elmnt.Note = BuildNote(midi);
                }

                return elmnt;
            }

            // Constrói um objeto Nota, interno a MeasureElement
            private Note BuildNote(int midiNote) {
                // Calcula representação interia de oitava e tom
                int octave = midiNote / 12;
                int pitch = midiNote % 12;

                // Define tom e modificador
                int tone = 0;
                Pitches p;
                switch (pitch) {
                    case 0:
                    case 1:
                        p = Pitches.C;
                        tone = pitch % 2;
                        break;

                    case 2:
                    case 3:
                        p = Pitches.D;
                        tone = pitch % 2;
                        break;

                    case 4:
                        p = Pitches.E;
                        break;

                    case 5:
                    case 6:
                        p = Pitches.F;
                        tone = -pitch % 2 + 1;
                        break;

                    case 7:
                    case 8:
                        p = Pitches.G;
                        tone = -pitch % 2 + 1;
                        break;

                    case 9:
                    case 10:
                        p = Pitches.A;
                        tone = -pitch % 2 + 1;
                        break;

                    case 11:
                        p = Pitches.B;
                        break;

                    default:
                        p = Pitches.NA;
                        break;
                }
                Note note = new Note(p, octave, tone);

                // Define papel na escala
                note.SetRole(_tonic);

                // Retorna nota;
                return note;
            }

            // Inicia novo instrumento
            public void NewPart() {
                _time = 0; // Reinicia contagem de tempo
                _part = new Part(); // Declara novo objeto instrumento
                _part.Measures = new List<Measure>();
                _measure = new Measure(); // Declara primeiro objeto compasso
                _position = 0;
                _measure.Elements = new List<MeasureElement>();
                _measure.Size = -1; // Marca tamanho do compasso como indefinido
            }

            // Adiciona instrumento à música
            public void AddPart() {
                _score.AddPart(_part);
            }

            // Inicia novo compasso (usado a partir do segundo compasso)
            public void NewMeasure() {
                _part.AddMeasure(_measure);
                _measure = new Measure();
                _measure.Elements = new List<MeasureElement>();
                _measure.Size = -1;
                _position = 0;
            }

            // Retorna objeto Score resultante
            public Score Return() {
                return _score;
            }
        }

        // Constrói objeto Score a partir de um MIDI
        public static Score FromMIDI(string midi) {

            using (Stream midiStream = File.OpenRead(midi)) {
                // Parser dos tream MIDI
                MidiSequence music = null;
                //try {
                    music = MidiSequence.Open(midiStream);
                //} catch (Exception ex) {
                //    Console.WriteLine(" Não foi possível abrir o arquivo ({0})", ex.Message);
                //    return null;
                //}

                MIDIBuilder.Instance.Reset();

                // Informação de Division
                MIDIBuilder.Instance.WholeNote = 4 * music.Division;
                
                // Itera sobre os Tracks do arquivo MIDI
                foreach (var track in music.Tracks) {

                    // Inicia novo instrumento
                    MIDIBuilder.Instance.NewPart();

                    // Itera Sobre os eventos de track
                    foreach (var evnt in track.Events) {
                        // Acumula tempo decorrido
                        MIDIBuilder.Instance.TimeElapsed += evnt.DeltaTime;

                        if (evnt is MidiSharp.Events.Meta.TimeSignatureMetaMidiEvent) {
                            MIDIBuilder.Instance.ChangeTimeSignature(evnt as MidiSharp.Events.Meta.TimeSignatureMetaMidiEvent);
                            MIDIBuilder.Instance.CheckForRest(true);

                        } else if (evnt is MidiSharp.Events.Voice.Note.OnNoteVoiceMidiEvent) {
                            MIDIBuilder.Instance.CheckForRest();
                            MIDIBuilder.Instance.OnNoteEvent(evnt as MidiSharp.Events.Voice.Note.OnNoteVoiceMidiEvent);

                        } else if (evnt is MidiSharp.Events.Voice.Note.OffNoteVoiceMidiEvent) { 
                            MIDIBuilder.Instance.OffNoteEvent(evnt as MidiSharp.Events.Voice.Note.OffNoteVoiceMidiEvent);

                        } else if ( evnt is MidiSharp.Events.Meta.KeySignatureMetaMidiEvent) {
                            MIDIBuilder.Instance.ChangeKey(evnt as MidiSharp.Events.Meta.KeySignatureMetaMidiEvent);
                            MIDIBuilder.Instance.CheckForRest();

                        } else {
                            continue;
                        }

                        MIDIBuilder.Instance.SaveLastEvent();
                    }

                    MIDIBuilder.Instance.AddPart();
                }

            }

            return MIDIBuilder.Instance.Return();
        }

        // Constrói objeto Score a partir de um musicXML
        public static Score FromXML(string musicXML) {
            Score score = new Score();
            //score.Parts = new List<Part>();

            foreach (var entry in ZipFile.OpenRead(musicXML).Entries) {
                bool isMusicXML = false;

                if (entry.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) {

                    StreamReader r = new StreamReader(entry.Open());
                    r.ReadLine();
                    if (r.ReadLine().Contains("score")) {
                        isMusicXML = true;
                    }
                    r.Close();

                    if (isMusicXML) {
                        entry.ExtractToFile(AppDomain.CurrentDomain.BaseDirectory + "temp.xml");
                    }
                }
            }

            MusicXml.Domain.Score xmlScore;
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "temp.xml")) {
                xmlScore = MusicXmlParser.GetScore(AppDomain.CurrentDomain.BaseDirectory + "temp.xml");
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "temp.xml");
            } else {
                throw (new Exception("Arquivo inválido"));
            }

            //Constrói as Partes
            Part newPart; //Parte a ser adicionada
            Measure newMeasure; //Compasso a ser adicionado
            MeasureElement newElement; //Elemento a ser adicionado

            //Auxiliares
            int divisions = -1;
            int fifths = 10;
            int beats = -1;
            int beatType = -1;
            int elementDuration = -1;
            int elementPosition = -1;
            int positionCounter = 0;
            Pitches notePitch = Pitches.NA;
            int noteOctave = -1;
            int noteTone  = -10;
            int partCounter = 0;
            int measureCounter = 0;
            int staveNumber = 0;
            int stave = 0;

            foreach (var part in xmlScore.Parts) {
                newPart = new Part();
                newPart.Measures = new List<Measure>();
                 
                foreach (var measure in part.Measures) {
                    newMeasure = new Measure();
                    newMeasure.Elements = new List<MeasureElement>();

                    //Atributos
                    if (measure.Attributes != null) {

                        if (measure.Attributes.Divisions > 0) {
                            divisions = measure.Attributes.Divisions;
                        }

                        if (measure.Attributes.Time.Beats > 0) {
                            beats = measure.Attributes.Time.Beats;
                        }

                        try {

                            if (measure.Attributes.Time.Mode != null && measure.Attributes.Time.Mode != string.Empty) {
                                beatType = int.Parse(measure.Attributes.Time.Mode);
                            }
                        } catch {
                            throw new Exception("Falha na identificação do tipo de Batida (ScoreBuilder.cs 56) -> pc:" + partCounter + "; mc:" + measureCounter);
                        }

                        if (measure.Attributes.Key != null) {
                            fifths = measure.Attributes.Key.Fifths;
                        }
                    }

                    if (divisions == -1 || beats == -1 || beatType == -1 || fifths == 10) {
                        throw new Exception("Erro na leitura do XML: DIVISIONS, BEATS, BEATTYPE ou FIFTHS não identificado (ScoreBuilder.cs 65)");
                    }
                    newMeasure.Division = divisions;
                    newMeasure.Size = ( int )(beats * divisions * (4.0 / beatType)); // (4 / beatType) = 1/beatType / 1/4; "converte" a batida em semínima
                    newMeasure.Tonic = IdentifyFifth(fifths);

                    //Elementos
                    positionCounter = 0;

                    foreach (var element in measure.MeasureElements) {
                        newElement = new MeasureElement();
                        
                        if (element.Type == MusicXml.Domain.MeasureElementType.Backup) {
                            var elmnt = element.Element as MusicXml.Domain.Backup;
                            Math.Max(0, positionCounter -= elmnt.Duration);
                        }

                        if (element.Type == MusicXml.Domain.MeasureElementType.Forward) {
                            var elmnt = element.Element as MusicXml.Domain.Forward;
                            positionCounter += elmnt.Duration;
                        }

                        if (element.Type == MusicXml.Domain.MeasureElementType.Note) {
                            var elmnt = element.Element as MusicXml.Domain.Note;
                            elementPosition = positionCounter;
                            elementDuration = elmnt.Duration;
                            
                            if (!elmnt.IsRest) {

                                if (elmnt.Pitch != null) {
                                    notePitch = IdentifyPitch(elmnt.Pitch.Step);
                                    noteOctave = elmnt.Pitch.Octave;
                                    noteTone = elmnt.Pitch.Alter;
                                } else {
                                    notePitch = Pitches.NA;
                                    noteOctave = 0;
                                    noteTone = 0;
                                }
                                newElement.Type = MeasureElementType.Note;
                                newElement.Note = new Note(notePitch, noteOctave, noteTone);
                                newElement.Note.SetRole(newMeasure.Tonic);
                            } else {
                                newElement.Type = MeasureElementType.Rest;
                            }
                            newElement.Duration = elementDuration;
                            newElement.Position = elementPosition;

                            if (elmnt.IsChordTone) {
                                newMeasure.AddChord(newElement);
                            } else {
                                if (newElement.Duration >= 0) {
                                    newMeasure.AddNote(newElement);
                                    positionCounter += elementDuration;
                                }
                            }
                        }
                    }
                    if (!newMeasure.Validate(measureCounter)) {
                        throw new Exception("Measure inválida (ScoreBuilder.cs 122) [pc:" + partCounter + "; mc:" + measureCounter + "; ec:" + newMeasure.Elements.Count + "]\n\t" + newMeasure.ToString());
                    }
                    newPart.AddMeasure(newMeasure);
                    measureCounter++;
                }
                score.AddPart(newPart);
                partCounter++;
                measureCounter = 0;
            }
            return score;
        }

        private static Pitches IdentifyPitch(char c) {
            switch (c) {
                case 'a':
                case 'A':
                    return Pitches.A;

                case 'b':
                case 'B':
                    return Pitches.B;

                case 'c':
                case 'C':
                    return Pitches.C;

                case 'd':
                case 'D':
                    return Pitches.D;

                case 'e':
                case 'E':
                    return Pitches.E;

                case 'f':
                case 'F':
                    return Pitches.F;

                case 'g':
                case 'G':
                    return Pitches.G;

                default:
                    return Pitches.NA;
            }
        }

        private static Pitches IdentifyFifth(int n) {
            switch (n) {
                case -7:
                    return Pitches.C;

                case -6:
                    return Pitches.G;

                case -5:
                    return Pitches.D;

                case -4:
                    return Pitches.A;

                case -3:
                    return Pitches.E;

                case -2:
                    return Pitches.B;

                case -1:
                    return Pitches.F;

                case 0:
                    return Pitches.C;

                case 1:
                    return Pitches.G;

                case 2:
                    return Pitches.D;

                case 3:
                    return Pitches.A;

                case 4:
                    return Pitches.E;

                case 5:
                    return Pitches.B;

                case 6:
                    return Pitches.F;

                case 7:
                    return Pitches.C;

                default:
                    return Pitches.NA;
            }
        }

    }
}
