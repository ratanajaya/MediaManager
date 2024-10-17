using Google.Api.Gax.ResourceNames;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translate.V3;
using Grpc.Auth;
using System.Linq;
using CoreAPI.AL.Models.Config;

namespace CoreAPI.AL.Services;
public class GoogleService
{
    GoogleCred _googleCred;

    public GoogleService(GoogleCred googleCred)
    {
        _googleCred = googleCred;
    }

    public string TranslateDetectLanguageToEnglish(string source) {
        var credential = GoogleCredential.FromServiceAccountCredential(
            new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(_googleCred.ClientEmail) {
                    ProjectId = _googleCred.ProjectId,
                    KeyId = _googleCred.PrivateKeyId,
                    Scopes = TranslationServiceClient.DefaultScopes,
                }.FromPrivateKey(_googleCred.PrivateKey)
            )
        );

        var clientBuilder = new TranslationServiceClientBuilder {
            ChannelCredentials = credential.ToChannelCredentials()
        };

        TranslationServiceClient client = clientBuilder.Build();

        // No need to specify SourceLanguageCode, it will auto-detect
        TranslateTextRequest request = new TranslateTextRequest {
            Contents = { source },
            TargetLanguageCode = "en",
            Parent = new ProjectName(_googleCred.ProjectId).ToString()
        };

        TranslateTextResponse response = client.TranslateText(request);

        if(!response.Translations.Any())
            return "Google's response.Translations is empty";

        return string.Join(' ', response.Translations.Select(a => a.TranslatedText));
    }
}
