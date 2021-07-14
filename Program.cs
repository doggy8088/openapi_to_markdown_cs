using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System.Text;

namespace openapi_to_markdown_cs
{
    class Program
    {
        static void Main(string[] args)
        {
            // var document = GenerateDocument();
            // var result = document.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
            // sb.AppendLine(result);
            // return;

            // var httpClient = new HttpClient
            // {
            //     BaseAddress = new Uri("https://raw.githubusercontent.com/OAI/OpenAPI-Specification/")
            // };
            // var stream = await httpClient.GetStreamAsync("master/examples/v3.0/petstore.yaml");

            using var stream = File.OpenRead("./swagger.json");

            // Read V3 as YAML
            var openApiDocument = new OpenApiStreamReader().Read(stream, out var diagnostic);

            // Write V2 as JSON
            // var outputString = openApiDocument.Serialize(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json);
            // Console.WriteLine(outputString);

            var sb = new StringBuilder();

            sb.AppendLine($"# {openApiDocument.Info.Title}");
            sb.AppendLine();

            foreach (var path in openApiDocument.Paths)
            {
                foreach (var op in path.Value.Operations)
                {
                    sb.AppendLine($"## {op.Key} {path.Key}");
                    sb.AppendLine();

                    var operation = op.Value;

                    var tags = String.Join(", ", operation.Tags.Select(t => t.Name));

                    var secu = operation.Security.FirstOrDefault()?.Keys.FirstOrDefault();

                    sb.AppendLine($"| 屬性名稱 | 屬性值              |");
                    sb.AppendLine($"| -------- | ------------------- |");
                    sb.AppendLine($"| 操作名稱 | {operation.Summary} |");
                    sb.AppendLine($"| 標　　籤 | {tags}              |");
                    sb.AppendLine($"| 安全機制 | {secu.BearerFormat} |");
                    sb.AppendLine($"| 權杖位置 | {secu.In}           |");
                    sb.AppendLine($"| 安全名稱 | {secu.Name}           |");
                    sb.AppendLine();

                    var sbParameters = new StringBuilder();
                    var hasParameter = false;

                    sbParameters.AppendLine($"### 參數清單");
                    sbParameters.AppendLine();
                    sbParameters.AppendLine($"| 參數名稱 | 參數位置 | 型別 | 格式 | Nullable | 說明 |");
                    sbParameters.AppendLine($"| -------- | -------- | ---- | ---- | -------- | ---- |");
                    foreach (var param in operation.Parameters)
                    {
                        sbParameters.Append($"| ");
                        sbParameters.Append(param.Name);
                        sbParameters.Append($" | ");
                        sbParameters.Append(param.In);
                        sbParameters.Append($" | ");
                        sbParameters.Append(param.Schema.Type);
                        sbParameters.Append($" | ");
                        sbParameters.Append(param.Schema.Format);
                        sbParameters.Append($" | ");
                        sbParameters.Append(param.Schema.Nullable);
                        sbParameters.Append($" | ");
                        sbParameters.Append(param.Schema.Description?.Replace("\r", "").Replace("\n", "<br>"));
                        sbParameters.AppendLine($" |");

                        hasParameter = true;
                    }
                    sbParameters.AppendLine();

                    if (hasParameter)
                    {
                        sb.Append(sbParameters.ToString());
                    }

                    var sbResponses = new StringBuilder();
                    var hasResponse = false;

                    sbResponses.AppendLine($"### 回應清單");
                    sbResponses.AppendLine();
                    sbResponses.AppendLine($"| 狀態碼   | 說明 | 內容類型 | 參考型別 |");
                    sbResponses.AppendLine($"| -------- | ---- | -------- | -------  |");
                    foreach (var res in operation.Responses)
                    {
                        var response = res.Value;

                        if (response.Content.Any())
                        {
                            var contentt = response.Content.FirstOrDefault(p => p.Key == "application/json");

                            sbResponses.Append($"| ");
                            sbResponses.Append(res.Key);
                            sbResponses.Append($" | ");
                            sbResponses.Append(response.Description.Replace("\n", " "));
                            sbResponses.Append($" | ");
                            sbResponses.Append(contentt.Key);
                            sbResponses.Append($" | ");
                            sbResponses.Append(contentt.Value.Schema.Type);
                            sbResponses.AppendLine($" |");

                            hasResponse = true;
                        }
                    }
                    sbResponses.AppendLine();

                    if (hasResponse)
                    {
                        sb.Append(sbResponses.ToString());
                    }

                }
            }

            System.Console.WriteLine(sb.ToString());

        }

        private static OpenApiDocument GenerateDocument()
        {
            return new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = "1.0.0",
                    Title = "Swagger Petstore (Simple)",
                },
                Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = "http://petstore.swagger.io/api" }
                },
                Paths = new OpenApiPaths
                {
                    ["/pets"] = new OpenApiPathItem
                    {
                        Operations = new Dictionary<OperationType, OpenApiOperation>
                        {
                            [OperationType.Get] = new OpenApiOperation
                            {
                                Description = "Returns all pets from the system that the user has access to",
                                Responses = new OpenApiResponses
                                {
                                    ["200"] = new OpenApiResponse
                                    {
                                        Description = "OK"
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
