using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace OCR_Model
{
    public class OcrDocument
    {
        //File path
        public string Path { get; set; }

        //File related properties
        public string Name { get; set; }
        public DateTime ScanDate { get; set; }

        //OCR related properties
        public string From { get; set; }
        public string To { get; set; }
        public string Topic { get; set; }
        public string Date { get; set; }
        public string DocumentType { get; set; }
    }

    public class OcrDocumentModel
    {
        public ObservableCollection<OcrDocument> OcrDocuments { get; set; }

        public OcrDocumentModel()
        {
            OcrDocuments = new ObservableCollection<OcrDocument>();           
        }

        public void Add(OcrDocument ocrDocument)
        {
            var docs = OcrDocuments.Where(doc => doc.Name.Equals(ocrDocument.Name));
            
            //check whether there is document with same name
            if (!docs.Any())
            {
                OcrDocuments.Add(ocrDocument);
            }
        }
    }
}
