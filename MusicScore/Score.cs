using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MusicScore
{
    public class Score {

        //Parts :: Lista de instrumentos ou partes que compoem a Música
        private List<Part> _parts;
        public List<Part> Parts {
            get { return _parts; }
            internal set { _parts = value; }
        }

        // Retorna parte (instrumento) que toca a menor quantidade de notas
        public Part LeastActive(bool countRests = false) {
            Part result = null;
            int reference = int.MaxValue;
            int count;

            foreach (Part part in _parts) {
                count = 0;

                foreach (Measure measure in part.Measures) {
                    foreach (MeasureElement element in measure.Elements) {
                        if (element.Type != MeasureElementType.Rest && !countRests) {
                            count++;
                        } else if (element.Type == MeasureElementType.Rest && countRests) {
                            count++;
                        }
                    }
                }

                if (count < reference) {
                    reference = count;
                    result = part;
                }
            }

            return result;
        }

        public List<int> RoleCounts() {
            List<int> count = new List<int>(7);
            int roleCount;

            roleCount = 0;
            foreach (var part in _parts) {
                roleCount += part.CountRole(Scale.Tonic);
            }
            count.Add(roleCount);

            roleCount = 0;
            foreach (var part in _parts) {
                roleCount += part.CountRole(Scale.Supertonic);
            }
            count.Add(roleCount);

            roleCount = 0;
            foreach (var part in _parts) {
                roleCount += part.CountRole(Scale.Mediant);
            }
            count.Add(roleCount);

            roleCount = 0;
            foreach (var part in _parts) {
                roleCount += part.CountRole(Scale.Subdominant);
            }
            count.Add(roleCount);

            roleCount = 0;
            foreach (var part in _parts) {
                roleCount += part.CountRole(Scale.Dominant);
            }
            count.Add(roleCount);

            roleCount = 0;
            foreach (var part in _parts) {
                roleCount += part.CountRole(Scale.Submediant);
            }
            count.Add(roleCount);

            roleCount = 0;
            foreach (var part in _parts) {
                roleCount += part.CountRole(Scale.Subtonic);
            }
            count.Add(roleCount);

            return count;
        }

        public int LeastActiveIndex(bool countRests = false) {
            int result = -1;
            int reference = int.MaxValue;
            int count;
            int partCount = 0;

            foreach (Part part in _parts) {
                count = 0;

                foreach (Measure measure in part.Measures) {
                    foreach (MeasureElement element in measure.Elements) {
                        if (element.Type != MeasureElementType.Rest && !countRests) {
                            count++;
                        } else if (element.Type == MeasureElementType.Rest && countRests) {
                            count++;
                        }
                    }
                }

                if (count < reference) {
                    reference = count;
                    result = partCount;
                }

                partCount++;
            }

            return result;
        }

        // Retorna parte (instrumento) que toca a maior quantidade de notas
        public Part MostActive(bool countRests = false) {
            Part result = null;
            int reference = int.MinValue;
            int count;

            foreach (Part part in _parts) {
                count = 0;

                foreach (Measure measure in part.Measures) {
                    foreach (MeasureElement element in measure.Elements) {
                        if (element.Type != MeasureElementType.Rest && !countRests) {
                            count++;
                        } else if (element.Type == MeasureElementType.Rest && countRests) {
                            count++;
                        }
                    }
                }

                if (count > reference) {
                    reference = count;
                    result = part;
                }
            }

            return result;
        }

        public int MostActiveIndex(bool countRests = false) {
            int result = -1;
            int reference = int.MinValue;
            int count;
            int partCount = 0;

            foreach (Part part in _parts) {
                count = 0;

                foreach (Measure measure in part.Measures) {
                    foreach (MeasureElement element in measure.Elements) {
                        if (element.Type != MeasureElementType.Rest && !countRests) {
                            count++;
                        } else if (element.Type == MeasureElementType.Rest && countRests) {
                            count++;
                        }
                    }
                }

                if (count > reference) {
                    reference = count;
                    result = partCount;
                }

                partCount++;
            }

            return result;
        }

        internal void AddPart(Part prt) {

            if(_parts == null) {
                throw new Exception("Tentativa de se adicionar ( Part )prt à uma lista não inicializada (Score.cs 21)");
            }
            _parts.Add(prt);
        }

        public void PrintPartWise(string path = null) {

            if (path != null) {

                using (StreamWriter partWise = new StreamWriter(path)) {

                    for (int pp = 0; pp < _parts.Count; pp++) {
                        partWise.WriteLine("PART {0}", pp);

                        for (int mm = 0; mm < _parts[pp].Measures.Count; mm++) {
                            partWise.Write("  M{0} : ", mm);
                            partWise.WriteLine(_parts[pp].Measures[mm].ToString());
                        }
                    }
                }
            } else {

                for (int pp = 0; pp < _parts.Count; pp++) {
                    Console.WriteLine("PART {0}", pp);

                    for (int mm = 0; mm < _parts[pp].Measures.Count; mm++) {
                        Console.Write("  M{0} : ", mm);
                        Console.WriteLine(_parts[pp].Measures[mm].ToString());
                    }
                }
            }
        }

        public void PrintTimeWise(string path = null) {
            
            if (path != null) {

                using (StreamWriter timeWise = new StreamWriter(path)) {

                    for (int mm = 0; mm < _parts[0].Measures.Count; mm++) {
                        timeWise.WriteLine("MEASURE {0}", mm);

                        for (int pp = 0; pp < _parts.Count; pp++) {
                            timeWise.Write("  P{0} : ", pp);
                            timeWise.WriteLine(_parts[pp].Measures[mm].ToString());
                        }
                    }
                }
            } else {
                for (int mm = 0; mm < _parts[0].Measures.Count; mm++) {
                    Console.WriteLine("MEASURE {0}", mm);

                    for (int pp = 0; pp < _parts.Count; pp++) {
                        Console.Write("  P{0} : ", pp);
                        Console.WriteLine(_parts[pp].Measures[mm].ToString());
                    }
                }
            }
        }

    }

}
