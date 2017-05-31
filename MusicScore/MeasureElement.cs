using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicScore {
    public class MeasureElement /*: IComparable*/ {

        //Position :: Indica a posição do Elemento no compasso
        private int _position;
        public int Position {
            get { return _position; }
            internal set { _position = value; }
        }

        //Duration :: Indica a duração da Elemento.
        private int _duration;
        public int Duration {
            get { return _duration; }
            internal set { _duration = value; }
        }

        //Type :: Indica tipo do elemento, nota, acorde ou pausa
        private MeasureElementType _type;
        public MeasureElementType Type {
            get { return _type; }
            internal set { _type = value; }
        }

        //Note :: A nota que o Elemento representa ou então NULL
        private Note _note;
        public Note Note {
            get { return _note; }
            internal set { _note = value; }
        }

        //IsNote :: Retorna TRUE se o elemento é uma nota
        public bool IsNote() {
            return _type == MeasureElementType.Note;
        }

        //IsChord :: Retorna TRUE se o elemento é um acorde
        public bool IsChord() {
            return _type == MeasureElementType.Chord;
        }

        //IsRest :: Retorna TRUE se o elemento é uma pausa
        public bool IsRest() {
            return _type == MeasureElementType.Rest;
        }

        public override string ToString() {
            string s = "";
            s += "<p:" + _position + ", ";
            s += "d:" + _duration + ">";

            if (_type == MeasureElementType.Rest) {
                s += "R";
            } else if (_type == MeasureElementType.Note) {
                s += "" + _note.Pitch + _note.Octave;
                s += "(" + _note.Tone + ")";
                s += "[" + _note.Role + "]";
            } else if (_type == MeasureElementType.ChordBase) {
                s += "" + _note.Pitch + _note.Octave + "*";
                s += "(" + _note.Tone + ")";
                s += "[" + _note.Role + "]";
            } else if (_type == MeasureElementType.Chord) {
                s += "+" + _note.Pitch + _note.Octave;
                s += "(" + _note.Tone + ")";
                s += "[" + _note.Role + "]";
            }
            return s;
        }

        //public int CompareTo(object obj) {
        //    return _position.CompareTo(obj);
        //}
    }

    public enum MeasureElementType {
        Note,
        Chord,
        ChordBase,
        Rest
    }
}
