using OpaqueMail.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail
{
    public class Attachment
	{
        public string Boundary;
        public string ContentId;
        public string MediaType;
        public string Name;
        public Encoding NameEncoding;
        public Stream ContentStream = new MemoryStream();

        public Attachment()
        {
        }

        public Attachment(string fileName)
        {
            FileInfo fi = new FileInfo(fileName);
            ContentStream = new MemoryStream(File.ReadAllBytes(fileName));
            ContentStream.Seek(0, SeekOrigin.Begin);
            Name = fi.Name;
        }

        public Attachment(string fileName, string mediaType)
        {
            FileInfo fi = new FileInfo(fileName);
            ContentStream = new MemoryStream(File.ReadAllBytes(fileName));
            ContentStream.Seek(0, SeekOrigin.Begin);
            Name = fi.Name;
            MediaType = mediaType;
        }

        public Attachment(string fileName, ContentType contentType)
        {
            FileInfo fi = new FileInfo(fileName);
            ContentStream = new MemoryStream(File.ReadAllBytes(fileName));
            ContentStream.Seek(0, SeekOrigin.Begin);
            Name = fi.Name;

            MediaType = contentType.MediaType;
            Boundary = contentType.Boundary;
        }

        public Attachment(Stream contentStream)
        {
            contentStream.CopyTo(ContentStream);
            ContentStream.Seek(0, SeekOrigin.Begin);
        }

        public Attachment(Stream contentStream, string name)
        {
            Name = name;
            contentStream.CopyTo(ContentStream);
            ContentStream.Seek(0, SeekOrigin.Begin);
        }

        public Attachment(Stream contentStream, string name, string mediaType)
        {
            Name = name;
            contentStream.CopyTo(ContentStream);
            ContentStream.Seek(0, SeekOrigin.Begin);
            MediaType = mediaType;
        }

        public Attachment(Stream contentStream, ContentType contentType)
        {
            contentStream.CopyTo(ContentStream);
            ContentStream.Seek(0, SeekOrigin.Begin);
            Name = contentType.Name;
            MediaType = contentType.MediaType;
            Boundary = contentType.Boundary;
        }

        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.Mail.AlternateView" /> and optionally releases the managed resources. </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
        protected void Dispose(bool disposing)
        {
        }
    }

    public class AttachmentCollection : Collection<Attachment>
    {
    }
}
