using System.Net.Http.Headers;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace GoogleDriveIntegration.src.Controllers;

public class DriveController : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/login", Login);
        app.MapGet("/login/callback", LoginCallback);
        app.MapPost("/upload", Upload).DisableAntiforgery();
        app.MapGet("/download/{fileId}", Download);
    }

    public static async Task<IResult> Login(IConfiguration config)
    {
        try{
            string clientId = config["GoogleDrive:ClientId"] ?? "";
            string redirectUri = config["GoogleDrive:RedirectURI"] ?? "";

            string url =
                "https://accounts.google.com/o/oauth2/v2/auth" +
                $"?client_id={clientId}" +
                $"&redirect_uri={redirectUri}" +
                $"&response_type=code" +
                $"&scope=https://www.googleapis.com/auth/drive" +
                $"&access_type=offline" +
                $"&prompt=consent";

            return Results.Redirect(url);
        }
        catch(Exception ex)
        {
            return Results.BadRequest(new {message = ex.Message, error = ex.ToString()});
        }
    }
    
    public static async Task<IResult> LoginCallback(IConfiguration config, HttpContext httpContext)
    {
        try
        {
            string code = httpContext.Request.Query["code"].ToString();

            string clientId = config["GoogleDrive:ClientId"] ?? "";
            string clientSecret = config["GoogleDrive:ClientSecret"] ?? "";
            string redirectUri = config["GoogleDrive:CallbackURI"] ?? "";

            using var client = new HttpClient();

            var response = await client.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                })
            );

            string json = await response.Content.ReadAsStringAsync();

            await File.WriteAllTextAsync("google-token.json", json);

            return Results.Ok(new {json});
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new {message = ex.Message, error = ex.ToString()});
        }
    }

    public static async Task<IResult> Upload(HttpContext httpContext, IHostEnvironment env, IConfiguration config, [FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("Arquivo inválido");

            string folderId = config["GoogleDrive:FolderId"] ?? "";
            DriveService drive = GetCredentials(config);

            using var stream = file.OpenReadStream();

            var metadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = file.FileName,
                Parents = new[] { folderId }
            };

            var request = drive.Files.Create(metadata, stream, file.ContentType);
            request.Fields = "id,name,mimeType,webViewLink";

            var result = await request.UploadAsync();

            if (result.Status != Google.Apis.Upload.UploadStatus.Completed)
            {
                return Results.Problem(
                    detail: result.Exception?.Message,
                    statusCode: 500
                );
            }

            return Results.Ok(new
            {
                Id = request.ResponseBody.Id,
                Name = request.ResponseBody.Name,
                Type = request.ResponseBody.MimeType,
                Url = request.ResponseBody.WebViewLink
            });
            
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new {message = ex.Message, error = ex.ToString()});
        }
    }
    
    public static async Task<IResult> Download(IConfiguration config, string fileId)
    {
        DriveService driveService = GetCredentials(config);

        var metaRequest = driveService.Files.Get(fileId);
        metaRequest.Fields = "name, mimeType";

        var metadata = await metaRequest.ExecuteAsync();

        MemoryStream stream = new ();

        var downloadRequest = driveService.Files.Get(fileId);
        await downloadRequest.DownloadAsync(stream);

        stream.Position = 0;

        // return Results.Stream( // Baixar o arquivo
        //     stream,
        //     metadata.MimeType ?? "application/octet-stream",
        //     metadata.Name
        // );
        return Results.Stream(
            stream,
            metadata.MimeType ?? "application/octet-stream",
            enableRangeProcessing: true
        );
    }


    // Doesnt work
    public static async Task<IResult> UploadByAccountService(IHostEnvironment env, IConfiguration config, [FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest(new { message = "Arquivo inválido" });

            string jsonPath = Path.Combine(env.ContentRootPath, "drive-integration-credential.json");
            string folderId = config["GoogleDrive:FolderId"] ?? "";

            GoogleCredential credential = GoogleCredential.FromFile(jsonPath).CreateScoped(DriveService.Scope.Drive);

            DriveService driveService = new (new BaseClientService.Initializer{
                HttpClientInitializer = credential,
                ApplicationName = "Minha API"
            });

            using var stream = file.OpenReadStream();

            Google.Apis.Drive.v3.Data.File metadata = new (){
                Name = file.FileName,
                Parents = new[] { folderId }
            };

            var request = driveService.Files.Create(
                metadata,
                stream,
                file.ContentType
            );

            request.Fields = "id, name, mimeType";

            var uploadResult = await request.UploadAsync();
            if (uploadResult.Status != Google.Apis.Upload.UploadStatus.Completed)
            {
                return Results.BadRequest(new
                {
                    message = "Upload não foi concluído",
                    status = uploadResult.Status.ToString(),
                    error = uploadResult.Exception?.Message,
                    details = uploadResult.Exception?.ToString()
                });
            }

            return Results.Ok(new
            {
                id = request.ResponseBody.Id,
                name = request.ResponseBody.Name,
                type = request.ResponseBody.MimeType
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new {message = ex.Message, error = ex.ToString()});
        }
    }

    private static DriveService GetCredentials(IConfiguration config)
    {
        string clientId = config["GoogleDrive:ClientId"] ?? "";
        string clientSecret = config["GoogleDrive:ClientSecret"] ?? "";
        string refreshToken = config["GoogleDrive:RefreshToken"] ?? "";

        GoogleAuthorizationCodeFlow flow = new (
            new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[] { DriveService.Scope.Drive }
            }
        );

        UserCredential credential = new (
            flow,
            "system-user",
            new TokenResponse
            {
                RefreshToken = refreshToken
            }
        );

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "My Project"
        });
    }

}