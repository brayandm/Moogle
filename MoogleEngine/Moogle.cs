using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoogleEngine
{
    //Clase Moogle: usada para hacer busquedas en documentos dada una query
    public class Moogle
    {
        //Este es el limite maximo de resultados a retornar
        static int TotalAnswerLimit = 10;
        //Este es el limite maximo de sinonimos a tener en cuenta de una palabra
        static int SynonymsLimit = 3;
        //Este es multiplicador de perdida al analizar sinonimos
        static double SynonymsRatioValue = 0.9;
        //Esta es la cantidad maxima de palabras de una seccion o particion de un documento
        static int MaxPieceSize = 100;
        //Este es el objeto que almacenara todas las palabras de todos los documentos
        //y nos ayudara a hacer busquedas de palabras similares
        static Dictionary Dic = new Dictionary();
        //Aqui se almacenara el conjunto de vectores de todos los documentos
        static VectorSpace Vspace = new VectorSpace();
        //Aqui se almacenara el tamanno de cada documento
        static Dictionary<string,int> DocSize = new Dictionary<string, int>();
        //Aqui se almacenara el conjunto de vectores de cada seccion o particion de cada documento
        static Dictionary<string,VectorSpace> DocVectorSpace = new Dictionary<string, VectorSpace>();
        //Aqui se almacenaran las posiciones de cada seccion o particion de cada documento
        static Dictionary<string,List<Tuple<int,int>>> DocRanges = new Dictionary<string,List<Tuple<int,int>>>();

        //Esta funcion es llamada al principio de todo
        //Aqui se hace el indexamiento de la informacion
        //Y se preprocesan las palabras y otros tipo de
        //informaciones
        public static void Init()
        {
            //Aqui se halla y se almacena los nombres de todos los documentos
            List<string> Names = TextPreprocessing.GetFileNamesFromDir("../Content/");

            //Aqui se almacena la informacion de los documentos
            List<Tuple<string,List<string>>> Data = new List<Tuple<string,List<string>>>();

            //Iterando por los nombres de los documentos
            foreach(string DocName in Names)
            {
                //Obteniendo palabras del documento
                List<string> Words = TextPreprocessing.GetWordsFromFile("../Content/" + DocName);

                //Iterando por las palabras del documento
                foreach(string Word in Words)
                {
                    //Annadiendo al Diccionario
                    Dic.AddWord(Word);
                }

                //Annadiendo a la informacion de los documentos
                Data.Add(new Tuple<string,List<string>>(DocName, Words));
            }
            
            //Creando conjunto de vectores segun la informacion de los documentos
            Vspace.BuildVectorSpace(Data);

            //Iterando por los nombres de los documentos
            foreach(string DocName in Names)
            {  
                //Aqui se almacena la informacion de las secciones o particiones de los documentos
                List<Tuple<string,List<string>>> DocData = new List<Tuple<string,List<string>>>();

                //Obteniendo palabras y sus posiciones de un archivo
                List<Tuple<string,int>> Words = TextPreprocessing.GetWordsAndPositionsFromFile("../Content/" + DocName);

                //Almacenando el tamanno del documento
                DocSize.Add(DocName, Words.Count);

                //Inicializando las posiciones de las secciones o particiones del documento
                DocRanges.Add(DocName, new List<Tuple<int,int>>());

                //Iterando por el documento
                for(int i = 0, DocNum = 0 ; i < Words.Count ; i += MaxPieceSize/2, DocNum++)
                {
                    //Aqui almacenaremos las palabras de la seccion o particion
                    List<string> PieceWords = new List<string>();

                    //Posicion donde empieza la seccion
                    int Start = Words[i].Item2;
                    //Posicion donde termina la seccion
                    int End = -1;

                    //Iterando por las palabras de la seccion
                    for(int j = i ; j < Math.Min(i+MaxPieceSize, Words.Count) ; j++)
                    {
                        //Annadiendo palabra
                        PieceWords.Add(Words[j].Item1);

                        //Esta variable almacena de bytes que ocupa la palabra
                        int size = 0;

                        //Recorriendo el string
                        foreach(char c in Words[j].Item1)
                        {
                            //Sumando los bytes
                            size += TextPreprocessing.NumberOfBytes((int)c);
                        }

                        //Actualizando final de la porcion
                        End = Words[j].Item2 + size - 1;
                    }

                    //Almacenando los rangos de la seccion o particion
                    DocRanges[DocName].Add(new Tuple<int, int>(Start, End));

                    //Annadiendo a la informacion de las secciones o particiones
                    DocData.Add(new Tuple<string,List<string>>(TextPreprocessing.NumerateFile(DocName,DocNum), PieceWords));
                }

                //Inicializando conjunto de vectores del documento
                DocVectorSpace.Add(DocName,new VectorSpace());

                //Creando conjunto de vectores del documento
                DocVectorSpace[DocName].BuildVectorSpace(DocData,Vspace.Winfo);
            }
        }

        //Esta funcion sirve para hallar los mejores resultados
        //Esta funcion retorna el nombre del documento, una pequenna
        //porcion del texto relacionada con la query, y el score
        //Primer parametro es la query
        //Segundo parametro son las palabras a buscar similitudes
        //Tercer parametro es un objeto de la clase SelectBestQuery que 
        //lo usaremos para mantener un control de palabras similares
        public static List<Tuple<string,string,double>> FindQuery(string query, List<string> Qu, SelectBestQuery bestQ)
        {
            //Creando el vector de la query con la informacion general
            Vector Vquery = new Vector(Qu, Vspace.Winfo);

            //Creando el parser de la query para obtener informacion sobre los operadores
            Parsing Parse = new Parsing(query, Qu, bestQ);

            //Buscando los mejores documentos relacionados con la query
            List<Tuple<string,double>> result = Vspace.FindSimilarDocVector(Vquery, Parse);

            //Aqui almacenaremos los mejores resultados
            List<Tuple<string,string,double>> Answer = new List<Tuple<string,string,double>>();

            //Iterando por los mejores documentos relacionados con la query
            for(int i = 0 ; i < result.Count ; i++)
            {
                //Buscar las mejores secciones o particiones relacionadas con la query
                List<Tuple<string,double>> DocResult = DocVectorSpace[result[i].Item1].FindSimilarDocVector(Vquery, Parse);

                //Esta variable se usa para saber si hemos procesado al menos una seccion o particion
                bool flag = false;
                
                //Inicializando
                string cad = "";
                double score = 0;

                //Iterando por cada seccion o particion del documento
                foreach(var DocR in DocResult)
                {
                    //Obteniendo el nombre del archivo y el numero de la seccion o particion
                    Tuple<string,int> Par = TextPreprocessing.GetFileAndNumber(DocR.Item1);

                    //Posicion inicial y final de la seccion o particion
                    int start = DocRanges[Par.Item1][Par.Item2].Item1;
                    int end = DocRanges[Par.Item1][Par.Item2].Item2;

                    //Convirtiendo de lista de string a string
                    string Pattern = TextPreprocessing.WordListToString(Qu);
                    
                    //Buscando la porcion del texto mas similar a un patron dado
                    Tuple<string,double> Ans = TextPreprocessing.FindMatchText("../Content/" + Par.Item1,start,end,Pattern,Vspace.Winfo,Parse);

                    //Si flag = false entonce igualamos el elemento
                    if(!flag)
                    {
                        flag = true;
                        cad = Ans.Item1;
                        score = Ans.Item2;
                    }
                    else
                    {
                        //Aqui nos quedamos con la seccion o particion que tenga mas score
                        if(score < Ans.Item2)
                        {
                            cad = Ans.Item1;
                            score = Ans.Item2;
                        }
                    }
                }
                
                //Almacenando la mejor porcion y su score
                //El score se calcula multiplicando el score
                //del documento por el de la porcion
                Answer.Add(new Tuple<string,string,double>(result[i].Item1, cad, result[i].Item2 * score));
            }

            return Answer;
        }

        //Esta funcion se utiliza para devolver las respuestas de la query
        public static SearchResult Query(string query)
        {
            //En caso de que no hayan palabras retornar una respuesta vacia
            if(Dic.GetSize() == 0)return new SearchResult();

            //Obteniendo palabras de la query
            List<string> Qu = TextPreprocessing.GetWordsFromString(query);

            //En caso de que la query no tenga palabras retornar una respuesta vacia
            if(Qu.Count == 0)return new SearchResult();

            //Creando el objeto de la clase SelectBestQuery para obtener las queries mas similares
            SelectBestQuery bestQ = new SelectBestQuery(Qu.Count);

            //Iterando por las palabras de la query
            for(int i = 0 ; i < Qu.Count ; i++)
            {
                //Iterando por las palabras similares buscadas en el diccionario
                foreach(var x in Dic.FindWord(Qu[i]))
                {
                    //Annadiendo palabra al objeto de la clase SelectBestQuery
                    //para luego poder mezclar esas palabras y generar queries similares
                    //Aqui calculamos un score que se basa en la similitud de dos palabras
                    //multiplicado por el IDF, mientras mas similares sean las palabras
                    //mejor prioridad se le deberia dar, de ahi la funcion 1000^x
                    bestQ.Insert(i,x.Item1,Math.Pow(1000,x.Item2) * Vspace.Winfo.GetIDF(x.Item1));

                    //Lo siguiente de abajo es para generar algunos sinonimos de las palabras
                    //En caso de haber internet pueden usarlo pero es un poco lento a la hora
                    //de hacer los requests

                    // Request request = new Request();

                    // List<string> lsyn = request.GetSynonyms(x.Item1);

                    // double rval = 1.0;

                    // for(int j = 0 ; j < Math.Min(SynonymsLimit, lsyn.Count) ; j++)
                    // {
                    //     rval *= SynonymsRatioValue;

                    //     if(Dic.Contais(lsyn[j]))
                    //     {
                    //         bestQ.Insert(i,lsyn[j],rval * Math.Pow(1000,x.Item2) * Vspace.Winfo.GetIDF(lsyn[j]));
                    //     }
                    // }
                }
            }

            //Aqui obtenemos las queries mas similares a la original
            List<Tuple<double,List<string>>> queries = bestQ.GetQueryList();

            //Aqui almacenaremos para cada documento la query que mas score tiene 
            Dictionary<string,Tuple<string,double,List<string>>> DocAnswer = new Dictionary<string, Tuple<string, double,List<string>>>();

            //Iterando por las queries
            foreach(var que in queries)
            {
                //Aqui obtenemos los resultados en un documento para una query dada
                List<Tuple<string,string,double>> Answer = FindQuery(query,que.Item2,bestQ);
                
                //Aqui iteramos por cada resultado
                foreach(var search in Answer)
                {
                    //Aqui calculamos el score que seria igual a el score de la
                    //busqueda en ese documento por el score de la query
                    //note que arriba habiamos hecho una exponencial, ahora
                    //usamos una logaritmica para suavizar el comportamiento general
                    double value = search.Item3 * Math.Log(que.Item1);

                    //Comprobando si hemos visto ese documento alguna vez
                    if(!DocAnswer.ContainsKey(search.Item1))
                    {
                        //En caso de que sea la primera vez igualarlo
                        DocAnswer.Add(search.Item1,new Tuple<string,double,List<string>>(search.Item2, value, que.Item2));
                    }
                    else
                    {
                        //Aqui comparamos por el que mayor score tenga y nos quedamos con el
                        if(value > DocAnswer[search.Item1].Item2)
                        {
                             DocAnswer[search.Item1] = new Tuple<string,double,List<string>>(search.Item2, value, que.Item2);
                        }
                    }
                }
            }

            //Aqui guardaremos la respuesta final
            List<Tuple<string,string,float,List<string>>> FinalAnswer = new List<Tuple<string,string,float,List<string>>>();

            //Iterando por los documentos
            foreach(var ans in DocAnswer)
            {
                //Agregando a la respuesta final
                FinalAnswer.Add(new Tuple<string,string,float,List<string>>(ans.Key,ans.Value.Item1,(float)ans.Value.Item2,ans.Value.Item3));
            }

            //Si no hay ningun resultado retornar una respuesta vacia
            if(FinalAnswer.Count == 0)return new SearchResult();

            //Ordenar los resultados segun su score
            FinalAnswer.Sort((x,y) => y.Item3.CompareTo(x.Item3));

            //Aqui nos quedamos con los mejores resultados y eliminamos los demas si nos pasamos del limite
            while(FinalAnswer.Count > TotalAnswerLimit)
            {
                FinalAnswer.RemoveAt(FinalAnswer.Count-1);
            }

            //Creando un array de SearchItem
            SearchItem[] items = new SearchItem[FinalAnswer.Count];

            //Iterando por los resultados
            for(int i = 0 ; i < FinalAnswer.Count ; i++)
            {
                //Igualando los resultados
                items[i] = new SearchItem(FinalAnswer[i].Item1 + " Score: " + FinalAnswer[i].Item3, FinalAnswer[i].Item2, FinalAnswer[i].Item3);
            }

            //Retornando los resultados y devolviendo la sugerencia de query con mas score
            return new SearchResult(items, TextPreprocessing.FixQueryWithNewWords(query, FinalAnswer[0].Item4));
        }
    }
}
