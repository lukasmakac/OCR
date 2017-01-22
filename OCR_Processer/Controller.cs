using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;
using OCR_Model;
using Tesseract;

namespace OCR_Controller
{
    public class Processer
    {
        const string Language = "eng";
        const string TesseractData = @".\tessdata\";

        // more under http://regexr.com/3f2i1 
        const string DatePattern =
            @"((?:\d\d?\w?\w?)|(?:January|February|March|April|May|June|July|August|September|November|December))[\s|/|,](\d\d?|(?:January|February|March|April|May|June|July|August|September|November|December))[\s|/|,]*(\d{4})";

        const string TopicPattern = @"Subject:\s((?:\w|\ )*)";
        const string FromPattern = @"(\w.*)\,?\n(?:\w.*\n){1,3}(?:.*)\n\n";
        const string ToPattern = @"To\:\s?(\w*)?";


        public Processer()
        {
        }

        /// <summary>
        /// Stores data related to document as name and scanDate
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="name">Filename of the document</param>
        /// <param name="scanDate">Date of the image was loaded into our system</param>
        public OcrDocument StoreFileRelatedDocumentData(string path, string name, DateTime scanDate)
        {
            OcrDocument ocrDocument = new OcrDocument
            {
                Path = path,
                Name = name,
                ScanDate = scanDate
            };


            return ocrDocument;
        }


        public OcrDocument Process(OcrDocument document)
        {
            using (var engine = new TesseractEngine(TesseractData, Language, EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(document.Path))
                {
                    using (var page = engine.Process(img))
                    {
                        ExtractInformation(document, page);

                        return document;
                    }
                }
            }
        }

        private void ExtractInformation(OcrDocument document, Page page)
        {
            var date = GetDate(page);
            if (date != null) document.Date = date;

            var topic = GetTopic(page);
            if (topic != null) document.Topic = topic;

            var from = GetFrom(page);
            if (from != null) document.From = from;

            var to = GetTo(page);
            if (to != null) document.To = to;
        }

        private String GetTopic(Page page)
        {
            String text = page.GetText();

            MatchCollection matches = GetRegexResult(TopicPattern, text);

            if (matches.Count > 0)
            {
                //return 1st group in 1st possible match 
                return matches[0].Groups[1].Value;
            }

            //return null if value wasn't found
            return null;
        }

        private String GetDate(Page page)
        {
            String text = page.GetText();

            MatchCollection matches = GetRegexResult(DatePattern, text);

            if (matches.Count > 0)
            {
                //return 1st group in 1st possible match 
                return matches[0].Value;
            }

            //return null if value wasn't found
            return null;
        }

        private String GetFrom(Page page)
        {

            using (var iter = page.GetIterator())
            {
                iter.Begin();


                do
                {
                    String text = iter.GetText(PageIteratorLevel.Block);
                    MatchCollection matches = GetRegexResult(FromPattern, text);

                    if (matches.Count > 0)
                    {
                        return matches[0].Groups[1].Value;
                    }
                } while (iter.Next(PageIteratorLevel.Block));

            }


            return null;
        }

        private String GetTo(Page page)
        {

            using (var iter = page.GetIterator())
            {
                iter.Begin();

                do
                {
                    String text = iter.GetText(PageIteratorLevel.TextLine);
                    MatchCollection matches = GetRegexResult(ToPattern, text);

                    if (matches.Count > 0)
                    {
                        //check whether To: information is in single line
                        if (!String.IsNullOrEmpty(matches[0].Groups[1].Value))
                        {
                            return matches[0].Groups[1].Value;
                        }
                        //find in first non-empty text line
                        else
                        {
                            while (iter.Next(PageIteratorLevel.TextLine))
                            {
                                String to = iter.GetText(PageIteratorLevel.TextLine);
                                if (!String.IsNullOrEmpty(to))
                                {
                                    //remove linefeed's symbols
                                    return to.Replace("\n","");
                                }
                            }
                        }                       
                    }
                } while (iter.Next(PageIteratorLevel.TextLine));

            }


            return null;
        }

        private MatchCollection GetRegexResult(String pattern, String text)
        {
            Regex defaultRegex = new Regex(pattern);

            // Get matches of pattern in text
            return defaultRegex.Matches(text);
        }
    }
}