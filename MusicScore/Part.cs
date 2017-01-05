using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicScore {
    public class Part {

        //Measures :: Lista de compassos do instrumento ou parte
        private List<Measure> _measures;
        public List<Measure> Measures {
            get { return _measures; }
            internal set { _measures = value; }
        }
        
        internal void AddMeasure(Measure msr) {

            if (_measures == null) {
                throw new Exception("Tentativa de se adicionar ( Measure )msr à uma lista não inicializada (Part.cs 20)");
            }
            _measures.Add(msr);
        }
    }
}
