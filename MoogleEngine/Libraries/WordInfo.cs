using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoogleEngine
{
    //Clase WordInfo: usada para mantener informacion sobre las palabras
    //asi como los mejores documentos donde aparece y el IDF de cada palabra
    public class WordInfo
    {
        //Limite de documentos similares a retornar
        static int BestDocsLimit = 10;
        //Total de documentos analizados
        static int TotDocs = 0;
        //Aqui almacenamos para cada palabra la cantidad de documentos en donde aparece
        public Dictionary<string,int> InDocs = new Dictionary<string,int>();
        //Aqui almacenamos para cada palabra los documentos con mas TF
        public Dictionary<string,List<Tuple<string,double>>> BestDocs = new Dictionary<string,List<Tuple<string,double>>>();

        //Esta funcion se usa para obtener el IDF de una palabra
        public double GetIDF(string word)
        {
            //Aplicamos la formula clasica del IDF y le sumamos un epsilon pequenno
            //para que no sea igual a cero
            return Math.Log((double)TotDocs/(double)InDocs[word]) + 0.001;
        }

        //Esta funcion sirve para insertar un documento y sus palabras
        //para actualizar la informacion
        public void Insert(string DocName, List<string> vect)
        {
            //Aumenta la cantidad de documentos
            TotDocs++;

            //Aqui se almacena para cada palabra la cantidad de veces que aparece
            Dictionary<string,int> Freq = new Dictionary<string,int>();
            //Aqui se almacena el conjunto de palabras distintas
            HashSet<string> Diff = new HashSet<string>();

            //Iterando sobre el conjunto inicial de palabras
            foreach(string word in vect)
            {
                //Analizando si la palabra ya fue vista
                if(!Diff.Contains(word))
                {
                    Diff.Add(word);
                    Freq.Add(word,0);
                }
                //Aumentando la cantidad de palabras iguales
                Freq[word]++;
            }

            //Funcion para obtener la frecuencia de una palaba
            //donde la frecuencia es la cantidad de palabras iguales
            //a la dada dividido entre el total de palabras
            double GetTF(string word)
            {
                return (double)Freq[word]/(double)vect.Count;
            }

            //Recorriendo el conjunto de palabras distintas
            foreach(string word in Diff)
            {
                //Analizando si la palabra ya fue vista en general
                if(!InDocs.ContainsKey(word))
                {
                    //Inicializando
                    InDocs.Add(word, 0);
                    BestDocs.Add(word, new List<Tuple<string, double>>());
                }

                //Aumentando la cantidad de veces que aparece la palabra
                InDocs[word]++;

                //Hallando el TF de la palabra
                double TF = GetTF(word);

                //Posicion donde insertaremos el nuevo documento que contiene a la palabra
                int pos = 0;

                //Iterando por los mejores documentos
                for(int i = 0 ; i < BestDocs[word].Count ; i++)
                {
                    //Si tiene menor TF deberia ir despues ya que queremos maximizar el TF
                    if(TF < BestDocs[word][i].Item2)
                    {
                        //Yendo a la siguiente posicion
                        pos++;
                    }
                    else break;
                }

                //Insertando en la posicion donde va
                BestDocs[word].Insert(pos, new Tuple<string,double>(DocName, TF));

                //Si se pasa del limite remover la ultima palabra
                if(BestDocs[word].Count > BestDocsLimit)
                {
                    BestDocs[word].RemoveAt(BestDocs[word].Count-1);
                }
            }
        }

        //Funcion para obtener mejores documentos que contienen una palabra dada
        public List<Tuple<string,double>> GetBestDocs(string word)
        {
            //Viendo si hay algun documentos para retornar
            if(BestDocs.ContainsKey(word))return BestDocs[word];
            return new List<Tuple<string,double>>();
        }
    }
}