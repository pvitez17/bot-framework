// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.14.0

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.IO;
using System.Net.Http;
using System;
using System.Configuration;

namespace BonsaiBot.Bots
{
    public class MainBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text != null) await AccessQnAMaker(turnContext, cancellationToken);
            else await AccessComputerVision(turnContext, cancellationToken);
        }


        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        public QnAMaker EchoBotQnA { get; private set; }

        public MainBot(QnAMakerEndpoint endpoint)
        {
            // connects to QnA Maker endpoint for each turn
            EchoBotQnA = new QnAMaker(endpoint);
        }

        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            return client;
        }

        List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
    {
        VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
        VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
        VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
        VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
        VisualFeatureTypes.Objects
    };

        private async Task AccessQnAMaker(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var results = await EchoBotQnA.GetAnswersAsync(turnContext);
            if (results.Any())
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(results.First().Answer), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Sorry, could not find an answer in the knowledge base."), cancellationToken);
            }
        }

        private async Task AccessComputerVision(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var result = new ImageAnalysis();
            var httpClient = new HttpClient();

            if (turnContext.Activity.Attachments?.Count > 0)
            {
                var attachment = turnContext.Activity.Attachments[0];
                var image = await httpClient.GetStreamAsync(new Uri(attachment.ContentUrl));
                if (image != null)
                {
                    result = await AnalyzeImageAsync(image);
                }
            }


            await turnContext.SendActivityAsync(MessageFactory.Text(result.Categories[0].Name), cancellationToken);
        }


        private async Task<ImageAnalysis> AnalyzeImageAsync(Stream image)
        {
            try
            {
                var endpoint = ConfigurationManager.AppSettings["ComputerVisionEndpoint"];
                var key = ConfigurationManager.AppSettings["ComputerVisionKey"];
                var client = Authenticate("https://bonsai-hiring-cv.cognitiveservices.azure.com", "e75b9326fb254197bc99a65a87a7a4d2");
                var analysis = await client.AnalyzeImageInStreamAsync(image, features);
                return analysis;
            }


            catch (Exception e)
            {
                throw e;
            }
        }
        
    }
}
    






