using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace MoogleEngine
{
    //Clase Request para obtener sinonimos de una palabra dada haciendo requests a un sitio web
    public class Request
    {
        //Aqui se guarda el respuesta
        static string responseBody = new string("");
        //El tiempo limite del request
        static int RequestTimeLimit = 5;
    
        //Funcion para hacer el request
        static async Task RequestSynonyms(string word)
        {
            //Creando cliente
            HttpClient client = new HttpClient();

            //Haciendo el request
            try	
            {
                //Fijar el limite del request
                client.Timeout = TimeSpan.FromSeconds(RequestTimeLimit);
                //Obteniendo respuesta
                responseBody = await client.GetStringAsync("http://sesat.fdi.ucm.es:8080/servicios/rest/sinonimos/json/" + word);
            }
            //Atrapar excepcion en caso de que se supere el limite de tiempo
            catch(TaskCanceledException e)
            {
                Console.WriteLine("\nException Caught!");	
                Console.WriteLine("Message :{0} ",e.Message);
                responseBody = new string("");
            }
            //Atrapar excepcion en caso de que haya algun error a la hora de hacer el request
            catch(HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");	
                Console.WriteLine("Message :{0} ",e.Message);
                responseBody = new string("");
            }
        }

        //Funcion igual que la anterior pero es mas obsoleta
        static void RequestSynonymsTwo(string word)
        {
            //Creando cliente
            var MyClient = new WebClient();
            //Obteniendo respuesta
            Stream response = MyClient.OpenRead("http://sesat.fdi.ucm.es:8080/servicios/rest/sinonimos/json/" + word);
            StreamReader reader = new StreamReader(response);
            //Trasformando a string e igualando
            responseBody = reader.ReadToEnd();
            //Cerrando el stream
            response.Close();
        }

        //Funcion que retorna lista de sinonimos dado una palabra
        public List<string> GetSynonyms(string word)
        {
            //Funciones de request

            //Descomentar aqui si se usa la primera funcion y luego comentar la de abajo
            //RequestSynonyms(word).Wait();

            //Segunda funcion de request
            RequestSynonymsTwo(word);

            //Si no hay respuesta no retornar nada
            if(responseBody.Length == 0)return new List<string>();

            //Parseando la respuesta removiendo posiciones sin informacion
            responseBody = responseBody.Remove(responseBody.Length-2,2);
            responseBody = responseBody.Remove(0,14);

            //Si esta vacio es que no hay sinonimos
            if(responseBody.Length == 0)return new List<string>();

            //Separar los sinonimos
            string[] arr = responseBody.Split(',');

            //Iterando por los sinonimos
            for(int i = 0 ; i < arr.Length ; i++)
            {
                //Obteniendo los sinonimos removiendo posiciones sin informacion
                arr[i] = arr[i].Remove(arr[i].Length-2,2);
                arr[i] = arr[i].Remove(0,13);
            }

            //Retornando sinonimos
            return new List<string>(arr);
        }
    }
}