// See https://aka.ms/new-console-template for more information
using CommandLine;
using CommandLine.Text;
using Docfx.Aspose.Tools;

#if DEBUG
//args = ["--help"];
//args = ["sitemap", "../../../../../../_site/sitemap.xml", "../../../../../docfx.json"];
args = ["verify", "../../../../../../_site/sitemap.xml", "http://localhost:8081"];
#endif

var parserResult = Parser.Default.ParseArguments<SitemapModeArgs, VerifyModeArgs>(args);

parserResult.MapResult(
    (SitemapModeArgs opts) => new SitemapProcessor(opts).Process(),
    (VerifyModeArgs opts) => new LinksVerifier(new HttpClient(), opts).VerifyAsync().AsSync(),
    _ => HandleParseError(parserResult));

///////////////////////////////////////////////

static int HandleParseError(ParserResult<object> parserResult)
{
    var helpText = HelpText.AutoBuild(parserResult);
    Console.WriteLine(helpText);

    return 1;
}