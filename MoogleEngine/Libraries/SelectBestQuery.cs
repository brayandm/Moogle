using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoogleEngine
{
    //Clase SelectBestQuery: se usa para generar queries similares a la original 
    //Recibe un conjunto de palabras y se generan una mezcla de ellas similares a la query original
    public class SelectBestQuery
    {
        //Limite de queries similares a retornar
        int QueryListLimit = 10;
        //Limite de palabras posibles a mezclar
        int WordsLimit = 20;
        //Aqui se guardan palabras similares a la i-esima palabra de la query
        public List<List<Tuple<string,double>>> arr = new List<List<Tuple<string,double>>>(); 

        //Constructor de la estructura
        //Aqui se le indica la cantidad de palabras de la query
        public SelectBestQuery(int tam)
        {
            //Iterando e inicializando
            for(int i = 0 ; i < tam ; i++)
            {
                arr.Add(new List<Tuple<string,double>>());
            }
        }
        //Insertando palabra similar a la i-esima palabra
        //Primer parametro es el indice de la palabra
        //Segundo parametro es la palabra similar
        //Tercer parametro es el score de similitud
        public void Insert(int pos, string cad, double score)
        {
            //Insertando palabra similar
            arr[pos].Add(new Tuple<string,double>(cad,score));
        }

        //Funcion para retornar la lista de queries similares
        public List<Tuple<double,List<string>>> GetQueryList()
        {
            //Lista para almacenar todos los scores de todas las palabras
            List<double> vect = new List<double>();

            //Ordenando por score las palabras similares para cada i-esima palabra
            for(int i = 0 ; i < arr.Count ; i++)
            {
                arr[i].Sort((x,y) => y.Item2.CompareTo(x.Item2));
            }

            //Aqui almacenaresmos todos los scores
            //Iterando por las palabras
            foreach(var vvv in arr)
            {
                //Iterando por las palabras similares
                foreach(var x in vvv)
                {
                    //Almacenando el score
                    vect.Add(x.Item2);
                }
            }

            //Ordenando los scores de menor a mayor;
            vect.Sort();
            //Virando la lista al reves para obtener de mayor a menor
            vect.Reverse();

            //Limite maximo score
            double LimitScore = vect[vect.Count-1];

            //Aqui limitamos el score de las 20 palabras con mas score
            //Esto se hace para solo escoger esas palabras y no ninguna otra
            if(WordsLimit < vect.Count)
            {
                LimitScore = vect[WordsLimit];
            }

            //Aqui almacenamos las queries y su similitud con la original
            List<Tuple<double,string[]>> Answer = new List<Tuple<double,string[]>>();

            //Lista temporal para usarla en la recursividad
            List<string> temp = new List<string>();

            //Funcion para generar mezclas de palabras para conformar queries
            //Primer parametro es el indice de la palabra
            //Segundo parametro es el score actual hasta ahora
            void GenerateCombs(int pos, double score)
            {
                //Chequando que llegamos al final
                if(pos == arr.Count)
                {
                    //Annadiendo query a la lista de queries
                    Answer.Add(new Tuple<double,string[]>(score, temp.ToArray()));
                    return;
                }

                //Iterando por palabras similares
                for(int i = 0 ; i < arr[pos].Count ; i++)
                {
                    //Aqui queremos garantizar que coja al menos las dos palabras mas similares
                    if(i < 2)
                    {
                        //Anndiendo a la lista
                        temp.Add(arr[pos][i].Item1);
                        //Ingresando en recursividad, saltando a la siguiente posicion y updateando el score
                        GenerateCombs(pos+1, score + arr[pos][i].Item2); 
                        //Eliminando de la lista
                        temp.RemoveAt(temp.Count-1);  
                    }
                    else
                    {
                        //Si la palabra tiene un score menor al limite no procesarla
                        if(arr[pos][i].Item2 > LimitScore)
                        {
                            //Anndiendo a la lista
                            temp.Add(arr[pos][i].Item1);
                            //Ingresando en recursividad, saltando a la siguiente posicion y updateando el score
                            GenerateCombs(pos+1, score + arr[pos][i].Item2); 
                            //Eliminando de la lista
                            temp.RemoveAt(temp.Count-1);  
                        }
                        else break;
                    }
                }
            }

            //Llamando a la funcion recursiva en indice 0 y teniendo score 0
            GenerateCombs(0,0);

            //Aqui almacenaremos las queries similares
            List<Tuple<double, List<string>>> TAnswer = new List<Tuple<double, List<string>>>();

            //Iterando pos las queries similares obtenidas en la recursividad anterior
            for(int i = 0 ; i < Answer.Count ; i++)
            {
                //Creando lista
                List<string> list = new List<string>();
                //Transformando de array a lista
                for(int j = 0 ; j < Answer[i].Item2.Length ; j++)
                {
                    list.Add(Answer[i].Item2[j]);
                }
                //Almacenando la query similar
                TAnswer.Add(new Tuple<double, List<string>>(Answer[i].Item1,list));
            }

            //Ordenando por las de mayor similitud
            TAnswer.Sort((x,y) => y.Item1.CompareTo(x.Item1));

            //Aqui nos quedamos con las mejores queries y eliminamos las demas si nos pasamos del limite
            while(TAnswer.Count > QueryListLimit)
            {
                TAnswer.RemoveAt(TAnswer.Count-1);
            }

            //Retornando queries similares
            return TAnswer;
        }
    }
}