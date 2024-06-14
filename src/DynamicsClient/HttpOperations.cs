﻿using System.Net;
using System.Text.Json;

namespace DynamicsClient;

internal class HttpOperations
{
    // ***********************************************************************************************************
    // Se declaran las variables necesarias para realizar la conexión a los servicios de Dynamics 365
    // ***********************************************************************************************************
    private readonly Uri _uri = new("http://190.249.146.250:81/OftalvisionLtda/api/data/v8.2/");
    private const string _domain = "<domain>";
    private const string _user = "<username>";
    private const string _password = "<password>";

    // ***********************************************************************************************************
    // Se realiza una solicitudo GET al servicio de Dynamics 365 y se obtiene su respuesta
    // ***********************************************************************************************************
    public async Task<List<T>?> GetCollectionAsync<T>(string entityName, string query) 
    {            
        var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(string.Concat(entityName, (!string.IsNullOrWhiteSpace(query) ? "?" : "") , query));
        
        // ***********************************************************************************************************
        // Si la respuesta a la solicitud no es correcta (o sea no está entre el rango 200), se termina el proceso.
        // ***********************************************************************************************************
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                throw new ApplicationException("No está autorizado para consultar la información de Dynamics 365.");

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new ApplicationException("No fue encontrado el recurso consultando la información de Dynamics 365.");

            throw new ApplicationException("Ocurrió un error consultado la información de Dynamics 365.");
        }

        // ***********************************************************************************************************
        // Si trata de obtener el contenido de la respuesta, y si está vacío se termina el proceso.
        // ***********************************************************************************************************
        string content = await response.Content.ReadAsStringAsync();
        ODataCollection<T>? oDataCollection = JsonSerializer.Deserialize<ODataCollection<T>>(content);
        if (oDataCollection?.Value == null)
            return default;

        return oDataCollection.Value;
    }

    // ***********************************************************************************************************
    // Se crea el cliente de conexión Http con los valores necesario para consumir el servicio de Dynamics 365
    // ***********************************************************************************************************
    private HttpClient GetHttpClient() 
    {
        // ***********************************************************************************************************
        // Se crea el objeto que administra las credenciales con las cuales se autenticará el servicio de Dynamics 365
        // ***********************************************************************************************************
        CredentialCache credentialsCache = new() 
        {
            { _uri, "Negotiate", new NetworkCredential(_user, _password, _domain) }
        };
        HttpClientHandler handler = new() { Credentials = credentialsCache, PreAuthenticate = true };
                    
        return new(handler) { BaseAddress = _uri, Timeout = new TimeSpan(0, 0, 60) };
    }
}