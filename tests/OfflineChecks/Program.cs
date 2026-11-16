using System.Text.Json;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Endpoint.App.Services.Conversation;
using MobileMessage = Endpoint.App.Models.Dtos.MessageDto;
using ServerMessage = ChatApp.Application.Services.Messages.MessageDto;
using ChatApp.Application.Services.Messages.Commands.SendMessage;
using ChatApp.Application.Services.Messages.Queries.GetMessage;

static void Check(bool condition, string label)
{
    if (!condition) throw new Exception(label);
    Console.WriteLine("PASS " + label);
}

var requestId = Guid.NewGuid();
var timeline = new MessageTimeline(42);
timeline.Merge(new MobileMessage { ConversationId = 42, ClientRequestId = requestId, Content = "hello", DeliveryState = "Sending" });
var saved = new MobileMessage { MessageId = 2, ConversationId = 42, ClientRequestId = requestId, SequenceNumber = 2, Content = "hello" };
timeline.Merge(saved);
timeline.Merge(saved);
Check(timeline.Messages.Count == 1 && timeline.Messages[0].MessageId == 2, "mobile pending/live/send reconciliation");
Check(timeline.RecoveryCursor == 0, "live events cannot advance mobile recovery cursor");
timeline.ApplyPage(new[] { new MobileMessage { MessageId = 1, ConversationId = 42, SequenceNumber = 1 }, saved });
Check(timeline.Messages.Select(m => m.MessageId).SequenceEqual(new long[] { 1, 2 }), "mobile sequence ordering and duplicate suppression");
Check(timeline.RecoveryCursor == 2, "completed history page advances mobile cursor");
timeline.ApplyPage(new[] { new MobileMessage { MessageId = 99, ConversationId = 7, SequenceNumber = 99 } });
Check(timeline.Messages.Count == 2 && timeline.RecoveryCursor == 2, "other conversations do not affect mobile state");

var entity = new ChatApp.Domain.Entities.Chat.Message
{
    Id = 2, ConversationId = 42, SenderId = 1, Content = "hello", ClientRequestId = requestId,
    SequenceNumber = 2, CreationDate = new DateTime(2026, 11, 16, 12, 0, 0)
};
var send = ServerMessage.FromEntity<ResultSendMessageDto>(entity);
var history = ServerMessage.FromEntity<GetConversationMessagesResultDto>(entity);
var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
Check(JsonSerializer.Serialize(send, jsonOptions) == JsonSerializer.Serialize(history, jsonOptions), "history/send wire contracts match exactly");
Check(send.SentAt.Kind == DateTimeKind.Utc && send.SentAt == entity.CreationDate, "persisted timestamps retain value and serialize as UTC");
var mobile = JsonSerializer.Deserialize<MobileMessage>(JsonSerializer.Serialize(send, jsonOptions), jsonOptions)!;
Check(mobile.MessageId == 2 && mobile.SenderId == 1 && mobile.ClientRequestId == requestId && mobile.SequenceNumber == 2,
    "mobile deserializes the server contract");

// Parse repository sources with the locally bundled compiler. This detects syntax
// errors only; it does not replace a full build with EF/MVVM/MAUI dependencies.
var root = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
var sourceFiles = Directory.GetDirectories(root).Where(p => Path.GetFileName(p).StartsWith("ChatApp.") ||
    Path.GetFileName(p).StartsWith("Endpoint.")).SelectMany(p => Directory.EnumerateFiles(p, "*.cs", SearchOption.AllDirectories))
    .Where(p => !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar) &&
        !p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)).ToList();
var errors = sourceFiles.SelectMany(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file), path: file)
    .GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error)).ToList();
foreach (var error in errors) Console.Error.WriteLine(error);
Check(errors.Count == 0, $"C# syntax across {sourceFiles.Count} source files (not a full build)");
foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "Endpoint.App"), "*.xaml", SearchOption.AllDirectories))
    XDocument.Load(file);
Check(true, "MAUI XAML is well-formed XML (not compiled bindings)");
