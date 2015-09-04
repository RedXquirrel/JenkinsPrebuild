using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Xamtastic.Patterns.CI.DropboxCI
{
    public class DropboxCIClient
    {
        private DropboxClient _client;

        #region Logging Actions
        public Action<string> LogMessageAction { get; set; }
        public Action<string> LogErrorAction { get; set; }

        private void LogMessage(string message)
        {
            if(LogMessageAction != null)
            {
                LogMessageAction.Invoke(message);
            }
        }

        private void LogError(string message)
        {
            if(LogErrorAction != null)
            {
                LogErrorAction.Invoke(message);
            }
        }
        #endregion

        public DropboxCIClient(string key)
        {
            _client = new DropboxClient(key);
        }

        public async Task Upload(string sourceFilePath, string dropboxFilePath)
        {

            string targetPath = dropboxFilePath;
            const int ChunkSize = 4096 * 1024;
            using (var fileStream = File.Open(sourceFilePath, FileMode.Open))
            {
                if (fileStream.Length <= ChunkSize)
                {
                    LogMessage(string.Format("File size <= 4MB ({0})", ChunkSize.ToString()));
                    LogMessage("Starting single file upload");
                    try
                    {
                        await this._client.Files.UploadAsync(targetPath, body: fileStream);
                    }
                    catch(Exception ex)
                    {
                        LogErrorAction(string.Format("ERROR: DropBoxClient UpLoadAsync error - {0}", ex.Message));
                    }
                }
                else
                {
                    LogMessage(string.Format("File size > 4MB ({0})", ChunkSize.ToString()));
                    LogMessage("Starting chunk session");
                    await this.ChunkUpload(targetPath, fileStream, ChunkSize);
                }
            }
        }

        private async Task ChunkUpload(String targetpath, FileStream sourceFileStream, int chunkSize)
        {
            int numChunks = (int)Math.Ceiling((double)sourceFileStream.Length / chunkSize);
            LogMessage(string.Format("Number of chunks calculated: {0}", numChunks.ToString()));
            byte[] buffer = new byte[chunkSize];
            string sessionId = null;
            for (var idx = 0; idx < numChunks; idx++)
            {
                var byteRead = sourceFileStream.Read(buffer, 0, chunkSize);

                using (var memSream = new MemoryStream(buffer, 0, byteRead))
                {
                    if (idx == 0)
                    {
                        LogMessage(string.Format("Chunk {0}: Commencing UploadSessionStartAsync", idx.ToString()));
                        try
                        {
                            var result = await this._client.Files.UploadSessionStartAsync(memSream);
                            sessionId = result.SessionId;
                        }
                        catch(Exception ex)
                        {
                            LogErrorAction(string.Format("ERROR: DropBoxClient UploadSessionStartAsync error - {0}", ex.Message));
                        }
                        LogMessage(string.Format("Chunk {0}: Session Id determined as [{1}]", idx.ToString(), sessionId));
                    }
                    else
                    {
                        var cursor = new UploadSessionCursor(sessionId, (ulong)(chunkSize * idx));
                        
                        if (idx == numChunks - 1)
                        {
                            LogMessage(string.Format("Chunk {0}: Commencing UploadSessionFinishAsync", idx.ToString()));
                            LogMessage(string.Format("Chunk {0}: Cursor Offset - {1}", idx.ToString(), cursor.Offset.ToString()));
                            try
                            {
                                await this._client.Files.UploadSessionFinishAsync(cursor, new CommitInfo(targetpath), memSream);
                            }
                            catch(Exception ex)
                            {
                                LogErrorAction(string.Format("ERROR: DropBoxClient UploadSessionFinishAsync error - {0}", ex.Message));
                            }
                            LogMessage(string.Format("Chunk {0}: Final Chunk uploaded", idx.ToString()));
                        }
                        else
                        {
                            LogMessage(string.Format("Chunk {0}: Commencing UploadSessionAppendAsync", idx.ToString()));
                            LogMessage(string.Format("Chunk {0}: Cursor Offset - {1}", idx.ToString(), cursor.Offset.ToString()));
                            try
                            {
                                await this._client.Files.UploadSessionAppendAsync(cursor, memSream);
                            }
                            catch(Exception ex)
                            {
                                LogErrorAction(string.Format("ERROR: DropBoxClient UploadSessionAppendAsync error - {0}", ex.Message));
                            }
                            LogMessage(string.Format("Chunk {0}: Chunk uploaded", idx.ToString()));
                        }
                    }
                }
            }
            LogMessage("Finishing chunk session");
        }
    }
}
