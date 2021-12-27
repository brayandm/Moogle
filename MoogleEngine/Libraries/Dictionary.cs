using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoogleEngine
{
    //Clase Diccionario: sirve para buscar el conjunto de palabras mas similares a una palabra dada
    //En esta estructura se recolectan todas las palabras contenidas en los documentos
    //La idea consiste en mantener para cada palabra algunas subpalabras obtenidas eliminando
    //algunos de sus caracteres, asi podemos localizar rapidamente palabras que difieran por pocos
    //caracteres
    public class Dictionary
    {
        //Aqui se guardan para cada subpalabra palabras posibles que la contienen como subsecuencia
        //Las mas similares van primero
        Dictionary<string,List<string>> SimilarWords = new Dictionary<string,List<string>>();
        //Aqui se guardan el conjunto de palabras de los documentos
        HashSet<string> MarkWord = new HashSet<string>();
        //Aqui se guardan el conjunto de subpalabras de las palabras de los documentos
        HashSet<string> MarkSubWord = new HashSet<string>();


        //Variables a continuacion se usan para ajustar el Rendimiento del Algoritmo:

        //Limite de palabras a la hora de eliminar algunos caracteres a la hora de annadir una palabra
        int AddWordErasingLimit = 100;
        //Limite de palabras a la hora de coger algunos caracteres a la hora de annadir una palabra
        int AddWordPickingLimit = 100;
        //Limite de palabras a la hora de eliminar algunos caracteres a la hora de buscar palabras similares
        int FindWordErasingLimit = 100;
        //Limite de palabras a la hora de coger algunos caracteres a la hora de buscar palabras similares
        int FindWordPickingLimit = 100;
        //Limite de palabras en que una subpalabra puede estar contenida
        int SimilarWordsLimit = 10;
        //Limite de palabras mas similares a retornar
        int BestWordsLimit = 10;
        //Limite en el prefijo de las palabras a la hora de tener en cuenta los lexemas
        int PrefixLimit = 8;
        //Limite de palabras a la hora de eliminar algunos caracteres a la hora de tener en cuenta los lexemas
        int PrefixErasingLimit = 100;

        //Se usa para ver si una palabra en especifico se encuentra dentro del diccionario
        public bool Contais(string word)
        {
            //Viendo si esta contenido en el conjunto de palabras 
            return MarkWord.Contains(word);
        }

        //Se usa para obtener el total de palabras en el diccionario
        public int GetSize()
        {
            //Obteniendo el tamanno del conjunto
            return MarkWord.Count();
        }

        //Esta funcion se usa para generar subpalabras de una palabra ya sea borrando algunos caracteres
        //o cogiendo algunos
        //Primer parametro es la palabra
        //Segundo parametro la cantidad de subpalabras a generar
        //Tercer parametro es para decir si queremos eliminar caracteres o coger algunos
        List<string> GenerateCombs(string word, int WordsCount, bool picking = false)
        {
            //Aqui se almacenan las subpalabras
            List<string> vect = new List<string>();
            //String temporal para ir generando las subpalabras en la recursividad
            StringBuilder temp = new StringBuilder();


            //Iterando sobre la cantidad de caracteres a eliminar o escoger
            for(int lim = 0 ; lim <= word.Length ; lim++)
            {
                //Funcion Recursiva para generar subpalabras
                //Primer parametro posicion actual en la palabra
                //Segundo parametro la cantidad de palabras borradas o escogidas
                void Generator(int pos, int erased)
                {
                    //Decir si ya alcanzamos el limite de subpalabras a generar
                    if(vect.Count == WordsCount)return;

                    //Decir si ya llegamos a la posicion final
                    if(pos == word.Length)
                    {
                        //Agregar a la lista de subpalabras
                        vect.Add(temp.ToString());
                        return;
                    }

                    //Si esta variable es true significa que estamos escogiendo algunos caracteres
                    //Si esta variable es false significa que estamos eliminando algunos caracteres
                    if(picking)
                    {
                        //Decir si podemos escoger otro caracter sin sobrepasar el limite
                        if(erased+1 <= lim)
                        {
                            //Agregando caracter actual al string temporal
                            temp.Append(word[pos]);
                            //Ingresando en recursividad
                            Generator(pos+1, erased+1);
                            //Eliminando caracter despues de la recursividad
                            temp.Length--;
                        }

                        //Decir si podemos eliminar este caracter verificando que al final cojamos
                        //una cantidad de caracteres iguales al limite
                        if(word.Length-pos > lim-erased)
                        {
                            //Ingresando en recursividad
                            Generator(pos+1, erased);
                        }
                    }
                    else
                    {
                        //Decir si podemos coger este caracter verificando que al final eliminemos
                        //una cantidad de caracteres iguales al limite
                        if(word.Length-pos > lim-erased)
                        {
                            //Agregando caracter actual al string temporal
                            temp.Append(word[pos]);
                            //Ingresando en recursividad
                            Generator(pos+1, erased);
                            //Eliminando caracter despues de la recursividad
                            temp.Length--;
                        }

                         //Decir si podemos eliminar otro caracter sin sobrepasar el limite
                        if(erased+1 <= lim)
                        {
                            //Ingresando en recursividad
                            Generator(pos+1, erased+1);
                        }
                    }
                }

                //Llamando funcion recursiva en posicion 0 y 0 eliminados o escogidos
                Generator(0, 0);
            }

            //Retornando lista de subpalabras
            return vect;
        }

        //Funcion para hallar la similitud entre dos palabras
        //Aqui nos enfocaremos en hallar el Edit Distance entre dos palabras
        //El Edit Distance es la minima cantidad de caracteres que debemos cambiar
        //agregar o eliminar para de una palabra obtener otra
        //Lo calcularemos usando programacion dinamica
        double Score(string s1, string s2)
        {
            //Tamanno de primera palabra
            int n = s1.Length;
            //Tamanno de segunda palabra
            int m = s2.Length;
            //dp[i,j] indica el Edit Distance entre el prefijo de tamanno i del primer string
            //y el prefijo de tamanno j del segundo string
            int[,] dp = new int[n+1,m+1];

            //A continuacion casos bases de la programacion dinamica
            //Usaremos la tecnica Bottom-Up

            //Edit Distance con prefijo del segundo string vacio
            for(int i = 0 ; i <= n ; i++)
            {
                dp[i,0] = i;
            }

            //Edit Distance con prefijo del primer string vacio
            for(int j = 0 ; j <= m ; j++)
            {
                dp[0,j] = j;
            }
            
            //Resolviendo de manera general
            for(int i = 1 ; i <= n ; i++)
            {
                for(int j = 1 ; j <= m ; j++)
                {
                    //Inicializando con la maxima respuesta posible
                    //Luego trataremos de minimizarla
                    dp[i,j] = Math.Max(n,m);
                    
                    //delta = 1 si el i-esimo caracter del primer string es igual
                    //al j-esimo del segundo, en otro caso delta = 0
                    int delta = 0;
                    if(s1[i-1] != s2[j-1])delta = 1;

                    //Resolviendo el Problema para dp[i,j] basandose en subproblemas
                    //dp[i-1,j], dp[i,j-1] y dp[i-1,j-1]

                    //Respuesta eliminando caracter i del primer string
                    dp[i,j] = Math.Min(dp[i,j], dp[i-1,j] + 1);
                    //Respuesta eliminando caracter j del segundo string
                    dp[i,j] = Math.Min(dp[i,j], dp[i,j-1] + 1);
                    //Respuesta matcheando caracter i del primer string
                    //con caracter j del segundo string si son iguales
                    //en caso contrario cambiar uno por el otro
                    dp[i,j] = Math.Min(dp[i,j], dp[i-1,j-1] + delta);
                }
            }
            //Hallando el porciento de correctitud
            //Esto significa Edit Distancia dividido entre
            //maxima posible respuesta y a su vez se lo restamos
            //a 1 para obtener la correctitud en vez de la incorrectitud
            return 1.0 - (double)dp[n,m]/(double)Math.Max(n,m);
        }

        //Para remover palabras duplicadas
        List<string> RemoveDuplicates(List<string> vect)
        {
            //Conviertiendo a HashSet para eliminar duplicados y luego llevarlo a lista
            return (new HashSet<string>(vect)).ToList();
        }

        //Funcion para annadir una palabra al Diccionario
        public void AddWord(string word)
        {
            //Ajustando la palabra a un formato mas ligero y facil de trabajar
            word = TextPreprocessing.ParseWord(word);
            
            //Viendo si ya la he annadido antes para evitar calculos
            if(MarkWord.Contains(word))return;
            //Introduciendo la palabra al conjunto de palabras
            MarkWord.Add(word);

            //Lista de subpalabras
            List<string> vect = new List<string>();

            //Generando subpalabras elimando caracteres
            vect.AddRange(GenerateCombs(word, AddWordErasingLimit));
            //Generando subpalabras escogiendo caracteres
            vect.AddRange(GenerateCombs(word, AddWordPickingLimit, true));
            //Generando subpalabras eliminando caracteres del prefijo o lexema
            vect.AddRange(GenerateCombs(word.Substring(0,Math.Min(word.Length,PrefixLimit)), PrefixErasingLimit));
            //Removiendo duplicados
            vect = RemoveDuplicates(vect);

            //Iterando por subpalabras
            foreach(string cad in vect)
            {
                //Insertando la subpalabra en el diccionario en caso de no haberlo hecho antes
                if(!MarkSubWord.Contains(cad))
                {
                    MarkSubWord.Add(cad);
                    SimilarWords.Add(cad,new List<string>());
                }

                //Posicion donde insertaremos la nueva palabra entre las similares de sus subpalabras
                int pos = 0;

                //Iterando por las similares de una subpalabra
                for(int i = 0 ; i < SimilarWords[cad].Count ; i++)
                {
                    //Si tiene mayor tamanno deberia ir despues ya que queremos minimizar el tamanno
                    if(word.Length > SimilarWords[cad][i].Length)
                    {
                        //Yendo a la siguiente posicion
                        pos++;
                    }
                    else break;
                }

                //Insertando en la posicion donde va
                SimilarWords[cad].Insert(pos, word);

                //Si se pasa del limite remover la ultima palabra
                if(SimilarWords[cad].Count > SimilarWordsLimit)
                {
                    SimilarWords[cad].RemoveAt(SimilarWords[cad].Count-1);
                }
            }
        }

        public List<Tuple<string,double>> FindWord(string word)
        {
            //Ajustando la palabra a un formato mas ligero y facil de trabajar
            word = TextPreprocessing.ParseWord(word);

            //Lista de subpalabras
            List<string> vect = new List<string>();

            //Generando subpalabras elimando caracteres
            vect.AddRange(GenerateCombs(word, FindWordErasingLimit));
            //Generando subpalabras escogiendo caracteres
            vect.AddRange(GenerateCombs(word, FindWordPickingLimit, true));
            //Generando subpalabras eliminando caracteres del prefijo o lexema
            vect.AddRange(GenerateCombs(word.Substring(0,Math.Min(word.Length,PrefixLimit)), PrefixErasingLimit));
            //Removiendo duplicados
            vect = RemoveDuplicates(vect);

            //Lista de palabras mas similares ordenadas por score de mayor a menor
            List<Tuple<string,double>> best = new List<Tuple<string,double>>();

            //Annadiendo palabra vacia
            best.Add(new Tuple<string,double>("",0));

            //Iterando sobre subpalabras
            foreach(string cad in vect)
            {
                //Viendo si esta en el conjunto de subpalabras
                if(MarkSubWord.Contains(cad))
                {
                    //Recorriendo palabras similares para una subpalabra
                    foreach(string s in SimilarWords[cad])
                    {
                        //flag = true si la palabra esta ya la analizamos y esta entre las mas similares
                        bool flag = false;

                        //Iterando por las mas similares
                        for(int i = 0 ; i < best.Count ; i++)
                        {
                            //Chequeando igualdad
                            if(s == best[i].Item1)flag = true;
                        }

                        //Si esta la palabra entre las similares saltar para la otra palabra
                        if(flag)continue;

                        //Calculando el score entre esta palabra y la palabra inicial
                        double sc = Score(word, s);


                        //Posicion donde insertaremos la nueva palabra entre las similares
                        int pos = 0;

                        //Iterando por las similares
                        for(int i = 0 ; i < best.Count ; i++)
                        {
                            //Si tiene menor score deberia ir despues ya que queremos maximizar el score
                            if(sc < best[i].Item2)
                            {
                                //Yendo a la siguiente posicion
                                pos++;
                            }
                            else break;
                        }

                        //Insertando en la posicion donde va
                        best.Insert(pos, new Tuple<string,double>(s,sc));

                        //Si se pasa del limite remover la ultima palabra
                        if(best.Count > BestWordsLimit)
                        {
                            best.RemoveAt(best.Count-1);
                        }
                    }
                }
            }

            //Retornando palabras mas similares a la inicial
            return best;
        }
    }
}