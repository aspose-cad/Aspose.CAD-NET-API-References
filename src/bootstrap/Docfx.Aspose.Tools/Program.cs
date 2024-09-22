﻿// See https://aka.ms/new-console-template for more information
using CommandLine;
using CommandLine.Text;
using Docfx.Aspose.Tools;

#if DEBUG
//args = ["--help"];
//args = ["sitemap", "../../../../../../_site/sitemap.xml", "../../../../../docfx.json"];
//args = ["verify", "../../../../../../_site/sitemap.xml", "http://localhost:8081"];
args = ["examples", "../../../../../../../../aspose.cad.net-2.0/src"];
#endif

var parserResult = Parser.Default.ParseArguments<SitemapModeArgs, VerifyModeArgs, ExamplesModeArgs>(args);

parserResult.MapResult(
    (SitemapModeArgs opts) => new SitemapProcessor(opts).Process(),
    (VerifyModeArgs opts) => new LinksVerifier(new HttpClient(), opts).VerifyAsync().AsSync(),
    (ExamplesModeArgs opts) => new ExamplesSourceValidator(opts).ProcessAsync().AsSync(),
    _ => HandleParseError(parserResult));

///////////////////////////////////////////////

static int HandleParseError(ParserResult<object> parserResult)
{
    var helpText = HelpText.AutoBuild(parserResult);
    Console.WriteLine(helpText);

    return 1;
}