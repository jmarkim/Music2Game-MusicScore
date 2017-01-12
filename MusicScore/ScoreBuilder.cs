using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicXml;

//PRBL :: Resolver partituras múltiplas (i.e. pianos com uma clave para cada mão)
//SLTN :: Ignorar pentagrama incorreto, loop for (1 a n) << possível super-adaptação >>

namespace MusicScore {
    public static class ScoreBuilder {

        //Constrói objeto Score a partir de um musicXML
        public static Score FromXML(string musicXML) {
            Score score = new Score();
            //score.Parts = new List<Part>();

            MusicXml.Domain.Score  xmlScore = MusicXmlParser.GetScore(musicXML);

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
                            positionCounter -= elmnt.Duration;
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
                                newMeasure.AddNote(newElement);
                                positionCounter += elementDuration;
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
