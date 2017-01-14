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
        const string DatePattern = @"((?:\d\d?\w?\w?)|(?:January|February|March|April|May|June|July|August|September|November|December))[\s|/|,](\d\d?|(?:January|February|March|April|May|June|July|August|September|November|December))[\s|/|,]*(\d{4})";
        const string TopicPattern = @"Subject:\s((?:\w|\ )*)";

        // storage
        // public ObservableCollection<OcrDocument> Documents { get; set; }

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
            String text = page.GetHOCRText(1);
            
            using (var iter = page.GetIterator())
            {
                iter.Begin();                

                while (iter.Next(PageIteratorLevel.Block))
                {

                    String line = iter.GetText(PageIteratorLevel.Block);
                    ElementProperties props = iter.GetProperties();
                    
                }
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

