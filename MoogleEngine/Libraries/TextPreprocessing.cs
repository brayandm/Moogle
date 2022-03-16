using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MoogleEngine
{

    //Clase TextPreprocessing: esta clase tiene multiples funciones para hacer trabajo con string 
    //y lecturas de archivos
    public class TextPreprocessing
    {
        //Funcion para saber la cantidad de bytes que ocupa un caracter segun el formato por default
        public static int NumberOfBytes(int x)
        {
            //Transformando de char a string
            string cad = "";
            cad += (char)x;
            //Contando Bytes
            return Encoding.Default.GetByteCount(cad);
        }

        //Chequeando si es un caracter de palabra
        public static bool IsWordCharacter(int x)
        {
            //Chequeando que este dentro del codigo ASCII o sea menor que 256
            //y usando la funcion para ver si es una letra o un numero
            if(x < (1<<8) && Char.IsLetterOrDigit((char)x))return true;
            return false;
        }

        //Funcion para ajustar la palabra a un formato mas ligero y facil de trabajar
        public static string ParseWord(string word)
        {
            //Retornando palabra en minusculas
            return word.ToLower();
        }

        //Funcion para numerar un particion de un archivo
        public static string NumerateFile(string FileName, int Number)
        {
            //Concatenando nombre mas numero de particion separados por guion
            return FileName + "-" + Number;
        }

        //Funcion para obtener el nombre del archivo y el numero de particion dada una concatenacion
        public static Tuple<string,int> GetFileAndNumber(string NumberFileName)
        {
            //Creando variables
            string FileName = "";
            int Number = 0;
            //Buscando el punto de particion desde atras hacia delante, en este caso 
            //seria buscar el guion mas cercano al final, esto es efectivo ya que antes
            //del ultimo guion solo pueden haber caracteres de numeros y con esto aseguramos 
            //que a partir de aqui se encuentra el numero de particion
            for(int i = NumberFileName.Length-1 ; i >= 0 ; i--)
            {
                //Localizando el punto de particion o sea el guion
                if(NumberFileName[i] == '-')
                {
                    //Obteniendo el nombre del archivo
                    FileName = NumberFileName.Substring(0,i);
                    //Obteniendo el numero de particion
                    Number = int.Parse(NumberFileName.Substring(i+1));
                    //Saliendo del ciclo
                    break;
                }
            }
            //retornando el nombre del archivo y el numero de particion
            return new Tuple<string, int> (FileName, Number);
        }

        //Funcion usada para remplazar la query original con palabras similares
        public static string FixQueryWithNewWords(string query, List<string> vect)
        {
            //Obteniendo palabras y posiciones de la query original
            List<Tuple<string,int>> arr = GetWordsAndPositionsFromString(query);

            //iterando desde atras hacia delante
            for(int i = arr.Count-1 ; i >= 0 ; i--)
            {
                //Aqui removemos la palabra que habia antes
                query = query.Remove(arr[i].Item2, arr[i].Item1.Length);
                //Aqui insertamos la nueva palabra sustituta
                query = query.Insert(arr[i].Item2, vect[i]);
                //Note que como iteramos de atras hacia delante las posiciones
                //absolutas de las palabras delanteras no se afectan cuando 
                //eliminamos o insertamos una palabra
            }

            //Retornando query con palabras remplazadas
            return query;
        }

        //Funcion para encontrar una porcion de un texto relacionada con un patron dado
        //Primer parametro es el directorio del archivo a obtener un texto
        //Segundo parametro es la posicion de inicio del texto en el archivo
        //Tercer parametro es la posicion de final del texto en el archivo
        //Cuarto parametro es el patron del cual queremos encontrar la porcion mas similar
        //Quinto parametro es la informacion sobre todas las palabras de los documentos
        //Sexto parametro es la informacion de los operadores de la query
        //La funcion retorna la porcion del texto mas similar al patron y su score
        public static Tuple<string,double> FindMatchText(string dir, int start, int end, string Pattern, WordInfo Winfo, Parsing Parse)
        {
            //Obteniendo palabras y posiciones del archivo
            List<Tuple<string,int>> TextWords = TextPreprocessing.GetWordsAndPositionsFromFile(dir, start, end);
            //Obteniendo lista de palabras del patron
            List<string> PatternWords = TextPreprocessing.GetWordsFromString(Pattern);

            //Maximo tamanno de una porcion
            int MatchSize = Math.Max(PatternWords.Count*3, 10);

            //Vector formado por el patron
            Vector Pvec = new Vector(PatternWords, Winfo);

            //Aqui almacenaremos el score y la posicion de la mejor porcion similar al patron
            double BestValue = -1;
            int BestStart = -1;
            int BestEnd = -1;

            //Numero de pares de string cercanos segun el operador '~' de cercania
            int NumberOfPairWords = 0;

            //Iterando por el texto de porcion en porcion
            for(int i = 0 ; i < TextWords.Count ; i+=MatchSize/6)
            {
                //Palabras en la porcion actual
                List<string> TextPiece = new List<string>();

                //Posicion inicial y final de la porcion
                int Start = TextWords[i].Item2;
                int End = -1;

                //Iterando por cada palabra de la porcion
                for(int j = i ; j < Math.Min(i+MatchSize, TextWords.Count) ; j++)
                {
                    //Annadiendo palabra a la lista de palabras de la porcion
                    TextPiece.Add(TextWords[j].Item1);

                    //Aqui almacenaremos su tamanno en bytes
                    int size = 0;

                    //Recorriendo el string
                    foreach(char c in TextWords[j].Item1)
                    {
                        //Sumando los bytes
                        size += TextPreprocessing.NumberOfBytes((int)c);
                    }

                    //Actualizando final de la porcion
                    End = TextWords[j].Item2 + size - 1;
                }

                //Vector formado por la porcion
                Vector Tvec = new Vector(TextPiece, Winfo);

                //Iterando por pares de string cercanos segun el operador '~' de cercania
                foreach(var P in Parse.PairWords)
                {
                    //Contando pares de string cercanos segun el operador '~' de cercania
                    if(Tvec.WordVector.ContainsKey(P.Item1) && Tvec.WordVector.ContainsKey(P.Item2))
                    {
                        NumberOfPairWords++;
                    }
                }

                //Score de similitud entre el patron y la porcion utilizando la similitud por coseno
                double Value = Tvec.CosineSimilarity(Pvec, Parse);

                //Quedandose con el mejor score
                if(Value > BestValue)
                {
                    BestValue = Value;
                    BestStart = Start;
                    BestEnd = End;
                }

                //Si ya procesamos la ultima palabra del texto entonces terminar
                if(Math.Min(i+MatchSize, TextWords.Count) == TextWords.Count)
                {
                    break;
                }
            }

            //Retornando la porcion del texto mas similar al patron y su score
            //el score lo calculamos multiplicando el score de la porcion por
            //el logaritmo mas 1 del numero pares de string cercanos segun el
            //operador '~' de cercania
            return new Tuple<string,double>(TextPreprocessing.GetStringFromFile(dir,BestStart,BestEnd), BestValue * (Math.Log(NumberOfPairWords+1) + 1));
        }

        //Funcion para convertir de una lista de string a un string concatenando los strings de la lista
        public static string WordListToString(List<string> vect)
        {
            //string temporal
            StringBuilder cad = new StringBuilder();

            //Iterando por strings
            for(int i = 0 ; i < vect.Count ; i++)
            {
                //Armando el string final
                if(i > 0)cad.Append(" ");
                cad.Append(vect[i]);
            }

            //Retornando el string final
            return cad.ToString();
        }

        //Funcion para obtener los nombres de los archivos en un directorio
        public static List<string> GetFileNamesFromDir(string dir)
        {
            //Obteniendo subdirectorios de los archivos de un directorio
            List<string> vect = new List<string>(Directory.GetFiles(dir, "*.txt", SearchOption.AllDirectories));
            //Iterando por los subdirectorios
            for(int i = 0 ; i < vect.Count ; i++)
            {
                //Obteniendo nombre del archivo
                vect[i] = vect[i].Substring(dir.Length);
            }
            //Retornando nombres de los archivos
            return vect;
        }

        //Funcion para obtener palabras y sus posiciones de un string
        public static List<Tuple<string,int>> GetWordsAndPositionsFromString(string cad)
        {
            //Creando lista para almacenar la respuesta
            List<Tuple<string,int>> vect = new List<Tuple<string,int>>();

            //posicion inicial de una palabra
            int last = -1;
            //variable temporal
            StringBuilder word = new StringBuilder();

            //iterando por el string
            for(int i = 0 ; i < cad.Length ; i++)
            {
                //Cogiendo solo los caracteres de palabra
                if(IsWordCharacter((int)cad[i]))
                {
                    //Inicializando posicion inicial
                    if(word.Length == 0)last = i;
                    //Agregar al string temporal
                    word.Append(cad[i]);
                }
                else
                {
                    //guardando palabra y su posicion
                    if(word.Length > 0)
                    {
                        vect.Add(new Tuple<string,int>(ParseWord(word.ToString()), last));
                        last = -1;
                        word.Clear();
                    }
                }
            }

            //guardando palabra y su posicion
            if(word.Length > 0)
            {
                vect.Add(new Tuple<string,int>(ParseWord(word.ToString()), last));
                last = -1;
                word.Clear();
            }

            //Retornando palabras y sus posiciones
            return vect;
        }

        //Funcion para obtener palabras y sus posiciones de un archivo
        public static List<Tuple<string,int>> GetWordsAndPositionsFromFile(string dir, int start = 0, int end = -1)
        {
            //Chequeando valores en el rango admisible
            if(start < 0 || end < -1)
            {
                return new List<Tuple<string,int>>();
            }
            //Chequeando valores en el rango admisible
            if(end != -1 && start > end)
            {
                return new List<Tuple<string,int>>();
            }

            //Abriendo archivo
            Stream stream = File.Open(dir, FileMode.Open);
            
            //Empezando en una posicion especifica
            stream.Seek(start, SeekOrigin.Begin);

            StreamReader reader = new StreamReader(stream);

            //Aqui se almacenara la respuesta
            List<Tuple<string,int>> vect = new List<Tuple<string,int>>();

            //posicion inicial de una palabra
            int last = -1;
            //Variable temporal
            StringBuilder word = new StringBuilder();

            //Iterando por el texto
            while(start <= end || end == -1)
            {
                //Leyendo caracter
                int c = reader.Read();

                //Chequeando si llegue al final
                if(reader.EndOfStream)
                {
                    break;
                }

                //Chequeando si es un caracter de palabra
                if(IsWordCharacter(c))
                {
                    if(word.Length == 0)last = start;
                    word.Append((char)c);
                }
                else
                {
                    //guardando palabra y su posicion
                    if(word.Length > 0)
                    {
                        vect.Add(new Tuple<string,int>(ParseWord(word.ToString()), last));
                        last = -1;
                        word.Clear();
                    }
                }

                //Aumentando posicion segun bytes
                start += NumberOfBytes(c);
            }

            //guardando palabra y su posicion
            if(word.Length > 0)
            {
                vect.Add(new Tuple<string,int>(ParseWord(word.ToString()), last));
                last = -1;
                word.Clear();
            }

            stream.Close();

            //Retornando palabras y sus posiciones
            return vect;
        }

        //Funcion para obtener un string de un archivo
        public static string GetStringFromFile(string dir, int start = 0, int end = -1)
        {
            //Chequeando valores en el rango admisible
            if(start < 0 || end < -1)
            {
                return "";
            }
            //Chequeando valores en el rango admisible
            if(end != -1 && start > end)
            {
                return "";
            }

            //Abriendo archivo
            Stream stream = File.Open(dir, FileMode.Open);
            
            //Empezando en una posicion especifica
            stream.Seek(start, SeekOrigin.Begin);

            StreamReader reader = new StreamReader(stream);

            //Variable temporal
            StringBuilder text = new StringBuilder();

            //Iterando por el texto
            while(start <= end || end == -1)
            {
                //Leyendo caracter
                int c = reader.Read();

                //Chequeando si llegue al final
                if(reader.EndOfStream)
                {
                    break;
                }

                //guardando caracter
                text.Append((char)c);

                //Aumentando posicion segun bytes
                start += NumberOfBytes(c);
            }

            stream.Close();

            //Retornando string
            return text.ToString();
        }

        //Funcion para obtener palabras de un archivo
        public static List<string> GetWordsFromFile(string dir, int start = 0, int end = -1)
        {
            //Chequeando valores en el rango admisible
            if(start < 0 || end < -1)
            {
                return new List<string>();
            }
            //Chequeando valores en el rango admisible
            if(end != -1 && start > end)
            {
                return new List<string>();
            }

            //Abriendo archivo
            Stream stream = File.Open(dir, FileMode.Open);
            
            //Empezando en una posicion especifica
            stream.Seek(start, SeekOrigin.Begin);

            StreamReader reader = new StreamReader(stream);

            //Lista de palabras de un archivo
            List<string> vect = new List<string>();

            //Variable temporal
            StringBuilder word = new StringBuilder();

            //Iterando por el texto
            while(start <= end || end == -1)
            {
                //Leyendo caracter
                int c = reader.Read();

                //Chequeando si llegue al final
                if(reader.EndOfStream)
                {
                    break;
                }

                //Chequeando si es un caracter de palabra
                if(IsWordCharacter(c))
                {
                    word.Append((char)c);
                }
                else
                {
                    //guardando palabra
                    if(word.Length > 0)
                    {
                        vect.Add(ParseWord(word.ToString()));
                        word.Clear();
                    }
                }

                //Aumentando posicion segun bytes
                start += NumberOfBytes(c);
            }

            //guardando palabra
            if(word.Length > 0)
            {
                vect.Add(ParseWord(word.ToString()));
                word.Clear();
            }

            stream.Close();

            //Retornando palabras
            return vect;
        }

        //Funcion para obtener palabras de un string
        public static List<string> GetWordsFromString(string cad)
        {
            //Aqui se almacenaran las palabras
            List<string> vect = new List<string>();

            //Variable temporal
            StringBuilder word = new StringBuilder();

            //iterando por el string
            for(int i = 0 ; i < cad.Length ; i++)
            {
                //Cogiendo solo los caracteres de palabra
                if(IsWordCharacter(cad[i]))
                {
                    word.Append(cad[i]);
                }
                else
                {
                    //guardando palabra
                    if(word.Length > 0)
                    {
                        vect.Add(ParseWord(word.ToString()));
                        word.Clear();
                    }
                }
            }

            //guardando palabra
            if(word.Length > 0)
            {
                vect.Add(ParseWord(word.ToString()));
                word.Clear();
            }

            //Retornando palabras
            return vect;
        }
    }
}
