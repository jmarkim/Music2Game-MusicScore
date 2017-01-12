using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicScore {
    public class Note {

        //Pitch :: O nome da Nota
        private Pitches _pitch;
        public Pitches Pitch {
            get { return _pitch; }
            internal set { _pitch = value; }
        }

        //Octave :: Oitava onde a Nota se situa
        private int _octave;
        public int Octave {
            get { return _octave; }
            internal set { _octave = value; }
        }

        //Tone :: Modificador de semitom da Nota, sutenido (+1), bemol (-1) ou natural (0). Outras variações também são suportados
        private int _tone;
        public int Tone {
            get { return _tone; }
            internal set { _tone = value; }
        }

        //Role :: "Papel" da nota na escala (i.e. Dominante, Tônica, Super-tônica...)
        private Scale _role;
        public Scale Role {
            get { return _role; }
            internal set { _role = value; }
        }

        //Construtor
        public Note(Pitches ptch, int octv, int tone, Scale role = Scale.NA) {
            Pitch = ptch;
            Octave = octv;
            Tone = tone;
            Role = role;
        }

        internal void SetRole(Pitches tonic) {
            int ptch = (PitchToInt(_pitch));
            if (ptch < 0) {
                _role = Scale.NA;
            }
            int sum = (7 + ptch - PitchToInt(tonic)) % 7;
            switch (sum) {
                case 0:
                    _role = Scale.Tonic;
                    break;

                case 1:
                    _role = Scale.Supertonic;
                    break;

                case 2:
                    _role = Scale.Mediant;
                    break;

                case 3:
                    _role = Scale.Subdominant;
                    break;

                case 4:
                    _role = Scale.Dominant;
                    break;

                case 5:
                    _role = Scale.Submediant;
                    break;

                case 6:
                    _role = Scale.Subtonic;
                    break;

                default:
                    _role = Scale.NA;
                    break;
            }
        }

        public static int PitchToInt(Pitches pitch) {
            switch (pitch) {
                case Pitches.C:
                    return 0;

                case Pitches.D:
                    return 1;

                case Pitches.E:
                    return 2;

                case Pitches.F:
                    return 3;

                case Pitches.G:
                    return 4;

                case Pitches.A:
                    return 5;

                case Pitches.B:
                    return 6;

                default:
                    return -1;
            }
        }
        
        public static int RoleToInt(Scale role) {
            switch (role) {
                case Scale.Tonic:
                    return 0;

                case Scale.Supertonic:
                    return 1;

                case Scale.Mediant:
                    return 2;

                case Scale.Subdominant:
                    return 3;

                case Scale.Dominant:
                    return 4;

                case Scale.Submediant:
                    return 5;

                case Scale.Subtonic:
                    return 6;

                default:
                    return -1;
            }
        }

        public static Scale IntToRole(int intRole) {
            switch (intRole) {
                case 0:
                    return Scale.Tonic;

                case 1:
                    return Scale.Supertonic;

                case 2:
                    return Scale.Mediant;

                case 3:
                    return Scale.Subdominant;

                case 4:
                    return Scale.Dominant;

                case 5:
                    return Scale.Submediant;

                case 6:
                    return Scale.Subtonic;

                default:
                    return Scale.NA;
            }
        }
    }

    public enum Pitches {
        C,
        D,
        E,
        F,
        G,
        A,
        B,
        NA
    }

    public enum Scale {
        Tonic,
        Supertonic,
        Mediant,
        Subdominant,
        Dominant,
        Submediant,
        Subtonic,
        NA
    }

}
