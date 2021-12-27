using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoogleEngine
{
    //Clase VectorSpace: usada para mantener organizado un conjunto
    //de vectores para poder hacer operaciones rapidas entre vectores
    //y buscar el que mas similitud tenga con respecto a otro vector dado
    public class VectorSpace
    {
        //Limite a retornar de vectores(documentos) similares a otro vector dado
        static int BestDocsLimit = 10;
        //Aqui almacenaremos los vectores de cada documento
        Dictionary<string,Vector> DocVector = new Dictionary<string,Vector>();

        //Aqui tendremos la informacion de todas las palabras de los documentos
        public WordInfo Winfo = new WordInfo();

        //Funcion para construir el conjunto de vectores
        //Recibe como entrada una lista de documentos con sus palabras
        public void BuildVectorSpace(List<Tuple<string,List<string>>> Vspace)
        {
            //Iterando por cada documento y sus palabras
            foreach(var doc in Vspace)
            {
                //Actualizando informacion sobre las palabras
                this.Winfo.Insert(doc.Item1, doc.Item2);
            }

            //Iterando por cada documento y sus palabras
            foreach(var doc in Vspace)
            {
                //Creando vector del documento
                this.DocVector.Add(doc.Item1, new Vector(doc.Item2, this.Winfo));
            }
        }

        //Funcion para construir el conjunto de vectores
        //Recibe como entrada una lista de documentos con sus palabras
        //y ademas recibe una objeto de la clase WordInfo que se usa
        //para crear los vectores con respecto a esa informacion y no
        //a la que se calcula cuando ejecutamos la funcion
        public void BuildVectorSpace(List<Tuple<string,List<string>>> Vspace, WordInfo totWinfo)
        {
            //Iterando por cada documento y sus palabras
            foreach(var doc in Vspace)
            {
                //Actualizando informacion sobre las palabras
                this.Winfo.Insert(doc.Item1, doc.Item2);
            }

            //Iterando por cada documento y sus palabras
            foreach(var doc in Vspace)
            {
                //Creando vector del documento
                this.DocVector.Add(doc.Item1, new Vector(doc.Item2, totWinfo));
            }
        }

        //Funcion usada para calcular los vectores(documentos) mas similares a un vector dado
        //Primer parametro es el vector al cual hallarle los vectores mas similares
        //Segundo parametro es la informacion de los operadores de la query
        public List<Tuple<string,double>> FindSimilarDocVector(Vector vect, Parsing Parse)
        {
            //Aqui obtenemos las palabras del vector recibido como parametro
            List<string> arr = vect.GetWords();

            //Aqui almacenaremos el conjunto de los documentos
            HashSet<string> Docs = new HashSet<string>();

            //Iterando por cada palabra del vector recibido como parametro
            foreach(string cad in arr)
            {
                //Iterando por los mejores documentos obtenidos del objeto de la clase WordInfo
                foreach(var x in this.Winfo.GetBestDocs(cad))
                {
                    //Chequeando si ya esta contenido o no
                    if(!Docs.Contains(x.Item1))
                    {
                        //Insertando si no estaba contenido
                        Docs.Add(x.Item1);
                    }
                }
            }

            //Aqui almacenaremos los mejores documentos y su score
            List<Tuple<string,double>> BestDocs = new List<Tuple<string,double>>();

            //Iterando por los documentos
            foreach(string doc in Docs)
            {
                //isOK = true indica que los documentos cumplen con las restricciones
                //de los operadores '^' y '!' en la query
                //isOK = false indica lo contrario
                bool isOK = true;

                //Iterando por las palabras que obligado deben aparecer
                foreach(string word in Parse.MustExistWords)
                {
                    //Chequeando si no esta presente en el documento
                    if(!this.DocVector[doc].WordVector.ContainsKey(word))
                    {
                        //isOK = false ya que se incumple la restriccion
                        isOK = false;
                    }
                }

                //Iterando por las palabras que obligado deben no aparecer
                foreach(string word in Parse.MustNotExistWords)
                {
                    //Chequeando si esta presente en el documento
                    if(this.DocVector[doc].WordVector.ContainsKey(word))
                    {
                        //isOK = false ya que se incumple la restriccion
                        isOK = false;
                    }
                }

                //Si isOK = true entonces agregamos el documento a los mejores documentos
                if(isOK)
                {
                    //Agregando a los mejores documentos
                    BestDocs.Add(new Tuple<string,double>(doc, this.DocVector[doc].CosineSimilarity(vect, Parse)));
                }
            }

            //Ordenando los documentos con respecto su score de mayor a menor
            BestDocs.Sort((x,y) => y.Item2.CompareTo(x.Item2));

            //Eliminando documentos si se pasan del limite impuesto
            while(BestDocs.Count > BestDocsLimit)
            {
                BestDocs.RemoveAt(BestDocs.Count-1);
            }

            //Retornando mejores documentos
            return BestDocs;
        }
    }
}