using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoogleEngine
{
    //Clase para Parsear la query y obtener la informacion sobre los operadores
    public class Parsing
    {
        //Palabras que deben existir
        public HashSet<string> MustExistWords = new HashSet<string>();
        //Palabras que no deben existir
        public HashSet<string> MustNotExistWords = new HashSet<string>();
        //Pares de palabras que cercanas aumentan el score
        public HashSet<Tuple<string,string>> PairWords = new HashSet<Tuple<string,string>>();
        //Para cada palabra su multiplicador de importancia
        public Dictionary<string,int> ImportanceWords = new Dictionary<string,int>();

        //Limite a la hora de obligar a palabras a existir entre las similares
        static int MustExistWordsLimit = 0;
        //Limite a la hora de obligar a palabras a no existir entre las similares
        static int MustNotExistWordsLimit = 1;
        //Limite a la hora de chequear cercania de palabras similares
        static int PairWordsLimit = 2;

        //Comparar dos palabras lexicograficamente
        public bool CompareWords(string s1, string s2)
        {
            //Comparar por caracter a caracter
            for(int i = 0 ; i < Math.Min(s1.Length, s2.Length) ; i++)
            {
                if(s1[i] < s2[i])return true;
                if(s1[i] > s2[i])return false;
            }

            //Si una es prefijo de otra comparar por tamanno
            if(s1.Length < s2.Length)return true;

            return false;
        }

        //Obtener la lista de posiciones en donde un caracter se encuentra en un string
        public List<int> GetCharPositions(string cad, char c)
        {
            //Lista de posiciones
            List<int> Pos = new List<int>();
            //Iterando sobre el string
            for(int i = 0 ; i < cad.Length ; i++)
            {
                //Chequeando igualdad
                if(cad[i] == c)
                {
                    //Annadiendo posicion
                    Pos.Add(i);
                }
            }
            //Retornando lista de posiciones
            return Pos;
        }

        //Dado una lista de palabras y sus posiciones retornar el menor indice de la palabra con 
        //posicion mayor que la dada
        public int FindFistRight(List<Tuple<string,int>> vect, int pos)
        {
            //Iterando de menor a mayor
            for(int i = 0 ; i < vect.Count ; i++)
            {
                //Chequeando
                if(pos < vect[i].Item2)
                {
                    return i;
                }
            }
            //No se encontro palabra
            return -1;
        }

        //Dado una lista de palabras y sus posiciones retornar el mayor indice de la palabra con 
        //posicion menor que la dada
        public int FindFistLeft(List<Tuple<string,int>> vect, int pos)
        {
            //Iterando de mayor a menor
            for(int i = vect.Count-1 ; i >= 0 ; i--)
            {
                //Chequeando
                if(pos > vect[i].Item2)
                {
                    return i;
                }
            }
            //No se encontro palabra
            return -1;
        }

        //Parseando query
        //Primer parametro la cadena a parsear
        //Segundo parametro la lista de string similares
        //Tercer parametro un objeto de la clase SelectBestQuery para obtener informacion de los similares
        public Parsing(string cad, List<string> list, SelectBestQuery bestQ)
        {
            //Obteniendo palabras y sus posiciones de un string
            List<Tuple<string,int>> vect = TextPreprocessing.GetWordsAndPositionsFromString(cad);

            //Buscando palabras que deberian estar obligado
            foreach(int pos in GetCharPositions(cad,'^'))
            {
                //Buscando primera palabra a la derecha
                int R = FindFistRight(vect, pos);

                //Si R = -1 no existe
                if(R != -1)
                {
                    //Agregarla al conjunto si no esta
                    if(!MustExistWords.Contains(list[R]))
                    {
                        MustExistWords.Add(list[R]);
                    }

                    //Iterando sobre similares para volverlas obligadas a pertenecer en el documento
                    for(int i = 0 ; i < Math.Min(MustExistWordsLimit, bestQ.arr[R].Count) ; i++)
                    {
                        //Agregarla al conjunto si no esta
                        if(!MustExistWords.Contains(bestQ.arr[R][i].Item1))
                        {
                            MustExistWords.Add(bestQ.arr[R][i].Item1);
                        }
                    }
                }
            }

            //Buscando palabras que no deben estar en el documento
            foreach(int pos in GetCharPositions(cad,'!'))
            {
                //Buscando primera palabra a la derecha
                int R = FindFistRight(vect, pos);

                //Si R = -1 no existe
                if(R != -1)
                {
                    //Agregarla al conjunto si no esta
                    if(!MustNotExistWords.Contains(list[R]))
                    {
                        MustNotExistWords.Add(list[R]);
                    }

                    //Iterando sobre similares para volverlas obligadas a no pertenecer en el documento
                    for(int i = 0 ; i < Math.Min(MustNotExistWordsLimit, bestQ.arr[R].Count) ; i++)
                    {
                        //Agregarla al conjunto si no esta
                        if(!MustNotExistWords.Contains(bestQ.arr[R][i].Item1))
                        {
                            MustNotExistWords.Add(bestQ.arr[R][i].Item1);
                        }
                    }
                }
            }

            //Inicializando importancia de palabras
            foreach(string word in list)
            {
                if(!ImportanceWords.ContainsKey(word))
                {
                    //Inicializando con valor neutro 1
                    ImportanceWords.Add(word, 1);
                }
            }

            //Buscando palabras operadores de importancia
            foreach(int pos in GetCharPositions(cad,'*'))
            {
                //Buscando primera palabra a la derecha
                int R = FindFistRight(vect, pos);

                //Si R = -1 no existe
                if(R != -1)
                {
                    //Duplicando la importancia de la palabra
                    ImportanceWords[list[R]] *= 2;
                }
            }

            //Buscando pares de palabras que cercanas aumentan el score
            foreach(int pos in GetCharPositions(cad,'~'))
            {
                //Buscando primera palabra a la izquierda
                int L = FindFistLeft(vect, pos);
                //Buscando primera palabra a la derecha
                int R = FindFistRight(vect, pos);

                //Si L = -1 o R = -1 entonces no podemos usar el operador
                if(L != -1 && R != -1)
                {
                    string s1 = list[L];
                    string s2 = list[R];

                    //Comparando palabras para volverlas una tupla ordenada
                    if(CompareWords(s2,s1))
                    {
                        (s1,s2) = (s2,s1);
                    }

                    //Insertando al set de pares palabras
                    if(!PairWords.Contains(new Tuple<string, string>(s1,s2)))
                    {
                        PairWords.Add(new Tuple<string, string>(s1,s2));
                    }

                    //Iterando por palabras similares a la primera
                    for(int i = 0 ; i < Math.Min(PairWordsLimit, bestQ.arr[L].Count) ; i++)
                    {
                        //Iterando por palabras similares a la segunda
                        for(int j = 0 ; j < Math.Min(PairWordsLimit, bestQ.arr[R].Count) ; j++)
                        {
                            s1 = bestQ.arr[L][i].Item1;
                            s2 = bestQ.arr[R][i].Item1;

                            //Comparando palabras para volverlas una tupla ordenada
                            if(CompareWords(s2,s1))
                            {
                                (s1,s2) = (s2,s1);
                            }

                            //Insertando al set de pares palabras
                            if(!PairWords.Contains(new Tuple<string, string>(s1,s2)))
                            {
                                PairWords.Add(new Tuple<string, string>(s1,s2));
                            }
                        }
                    }
                }
            }
        }
    }
}