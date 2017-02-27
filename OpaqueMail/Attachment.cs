/*
 * OpaqueMail (https://opaquemail.org/).
 * 
 * Licensed according to the MIT License (http://mit-license.org/).
 * 
 * Copyright © Bert Johnson (https://bertjohnson.com/) of Allcloud Inc. (https://allcloud.com/).
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace OpaqueMail
{
    public class Attachment
	{
        public string Boundary { get; set; }
        public string ContentId { get; set; }
        public string MediaType { get; set; }
        public string Name { get; set; }
        public Encoding NameEncoding { get; set; }
        public Stream ContentStream { get; set; }

        public Attachment()
        {
            ContentStream = new MemoryStream();
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
            : this()
        {
            contentStream.CopyTo(ContentStream);
            ContentStream.Seek(0, SeekOrigin.Begin);
        }

        public Attachment(Stream contentStream, string name)
            : this()
        {
            Name = name;
            contentStream.CopyTo(ContentStream);
            ContentStream.Seek(0, SeekOrigin.Begin);
        }

        public Attachment(Stream contentStream, string name, string mediaType)
            : this()
        {
            Name = name;
            contentStream.CopyTo(ContentStream);
            ContentStream.Seek(0, SeekOrigin.Begin);
            MediaType = mediaType;
        }

        public Attachment(Stream contentStream, ContentType contentType)
            : this()
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
            if (ContentStream != null)
                ContentStream.Dispose();
        }
    }

    public class AttachmentCollection : Collection<Attachment>
    {
    }
}
