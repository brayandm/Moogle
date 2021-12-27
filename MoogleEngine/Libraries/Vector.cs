using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoogleEngine
{
    //Clase Vector: usada para transformar un conjunto de palabras en un vector
    //La idea en este caso consiste en tratar cada palabra como una dimension
    //En cada dimension guardamos el TF-IDF de la palabra correspondiente a esa
    //dimension
    public class Vector
    {
        //Aqui se almacena para cada dimension su valor correspondiente
        public Dictionary<string,double> WordVector = new Dictionary<string,double>();
        //Aqui almacenamos la suma de los cuadrados de los valores de cada dimension
        double SumOfSquares = 0;

        //Constructor de la clase:
        //Primer parametro es el conjunto de palabras
        //Segundo parametro es la informacion sobre todas las palabras de los documentos
        public Vector(List<string> vect, WordInfo WordI)
        {
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
                //Hallando el TF-IDF de cada palabra
                double TF_IDF = GetTF(word)*WordI.GetIDF(word);
                //Almacenando el TF-IDF para cada palabra
                this.WordVector.Add(word, TF_IDF);
                //Actualizando la suma de los cuadrados de los valores de cada dimension 
                this.SumOfSquares += TF_IDF*TF_IDF;
            }
        }

        //Funcion para obtener las palabras del vector
        public List<string> GetWords()
        {
            //Aqui se almacenaran las palabras
            List<string> vect = new List<string>();

            //Iterando por las dimensiones del vector
            foreach(var x in WordVector)
            {
                //Annadiendo a la lista
                vect.Add(x.Key);
            }

            //Retornando la lista
            return vect;
        }

        //Funcion para calcular el coseno del angulo entre dos vectores
        //Primer parametro es el otro vector al que queremos hallarle el coseno
        //con respecto al actual
        //Segundo parametro es la informacion de los operadores en la query
        //Para hallar el coseno utilizaremos la formula de producto punto 
        //dividido entre la multiplicacion de los modulos de los dos vectores
        public double CosineSimilarity(Vector vec, Parsing Parse)
        {
            //Aqui almacenaremos el coseno del angulo entre los dos vectores
            double sum = 0;

            //Recorriendo las dimensiones del vector recibido como parametro
            //Este al ser el vector de la query tiene pocas dimensiones
            //Solo necesitamos procesar las dimensiones que sabemos que tienen
            //un valor distinto de cero ya que en otro caso el producto en
            //esa dimension seria cero
            foreach(var element in vec.WordVector)
            {
                //Analizando si esa dimension es distinta de cero en el vector actual
                if(this.WordVector.ContainsKey(element.Key))
                {
                    //Hallando la suma de los productos
                    sum += element.Value * this.WordVector[element.Key] * Parse.ImportanceWords[element.Key];
                }
            }

            //Dividiendo entre los modulos
            sum /= Math.Sqrt(vec.SumOfSquares * this.SumOfSquares);
            
            //Retornando coseno
            return sum;
        }
    }
}