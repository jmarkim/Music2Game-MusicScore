using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicScore {
    public class Measure {

        //Divisions :: Indica em quantas partes foi divida uma Semínima
        private int _division;
        public int Division {
            get { return _division; }
            internal set { _division = value; }
        }

        //Tonic :: Indica a tônica da escala. Assume esala maior
        private Pitches _tonic;
        public Pitches Tonic {
            get { return _tonic; }
            internal set { _tonic = value; }
        }

        //Size :: O tamanho do Compasso. A soma das durações dos elementos não podem ultrapassar esse valor
        private int _size;
        public int Size {
            get { return _size; }
            internal set { _size = value; }
        }

        //Complete :: Indica se o Compasso está completo (identifica anacrusis e sua finalisação)
        private bool _complete;
        public bool Complete {
            get { return _complete; }
            internal set { _complete = value; }
        }

        //Elements :: Lista de elementos do Compasso, notas, pausas e acordes
        private List<MeasureElement> _elements;
        public List<MeasureElement> Elements {
            get { return _elements; }
            internal set { _elements = value; }
        }

        internal void AddNote(MeasureElement element) {

            if (_elements == null) {
                throw new Exception("Tentativa de se adicionar ( MeasureElement )element à uma lista não inicializada (Measure.cs 48)");
            }
            _elements.Add(element);
        }

        internal void AddChord(MeasureElement element) {
            if (_elements == null) {
                throw new Exception("Tentativa de se adicionar ( MeasureElement )element à uma lista não inicializada (Measure.cs 55)");
            }

            MeasureElement lastElement = _elements.Last();

            element.Position = lastElement.Position;
            if (lastElement.Type == MeasureElementType.Note) {
                lastElement.Type = MeasureElementType.ChordBase;
            }
            element.Type = MeasureElementType.Chord;
            _elements.Add(element);
        }

        internal bool Validate(int measureNumber) {
            int sum = 0;
            int positionCounter = 0;
            
            foreach (MeasureElement elmnt in _elements) {

                if (positionCounter == elmnt.Position) {
                    sum += elmnt.Duration;
                    positionCounter += elmnt.Duration;
                }
            }

            if (sum < _size) {
                _complete = false;
                return true;
            } else if (sum == _size) {
                _complete = true;
                return true;
            } else {
                return false;
            }
        }

        public override string ToString() {
            string s = "";
            s += "<" + _tonic + "> ";
            s += "[s:" + _size + (_complete?"c":"i") + ", ";
            s += "d:" + _division + "]";
            s += " >>";

            foreach (MeasureElement elmnt in _elements) {
                s += " " + elmnt.ToString();
            }
            return s;
        }
    }
}
