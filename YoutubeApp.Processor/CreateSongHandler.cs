using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using JustSaying.Messaging.MessageHandling;
using YoutubeApp.Domain;

namespace YoutubeApp.Processor
{
    public class CreateSongCommandHandler : IHandlerAsync<CreateSongCommand>
    {
        public async Task<bool> Handle(CreateSongCommand message)
        {
            Console.WriteLine("processing " + message.Url);
            if (message.Url == null)
            {
                return true;
            }

            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            var workingDir = Directory.GetCurrentDirectory();
            var filename = Guid.NewGuid();
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                RedirectStandardError = true,
                UseShellExecute = false,
                FileName = "you-get",
                WorkingDirectory = workingDir,
                Arguments = $"  {message.Url} -O {filename}"
            };
            var exeProcess = Process.Start(startInfo);
            var isVideoExist = false;
            var filenameWithExtension = "";
            var title = "";

            while (!exeProcess.StandardOutput.EndOfStream)
            {
                string line = exeProcess.StandardOutput.ReadLine();
                if (Debugger.IsAttached)
                {
                    Console.WriteLine(line);
                }

                if (line.StartsWith("title:"))
                {
                    title = line.Substring(6).Trim();
                }

                if (line.StartsWith("Downloading"))
                {
                    filenameWithExtension = line.Substring(11).TrimEnd('.').Trim();
                }

                if (line.StartsWith("Skipping"))
                {
                    isVideoExist = true;
                    filenameWithExtension = line.Substring(11);
                }
            }

            var audioFileName = filename + ".mp3";
            if (!string.IsNullOrWhiteSpace(filenameWithExtension))
            {
                var startInfo2 = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "ffmpeg",
                    WorkingDirectory = workingDir,
                    Arguments = $" -i \"{filenameWithExtension}\" -vn -f mp3 -ab 192k \"{audioFileName}\""
                };
                using (var ffmpegProcess = Process.Start(startInfo2))
                {
                    ffmpegProcess.WaitForExit();
                }

                UploadFileToS3(filenameWithExtension, audioFileName, filename.ToString(), title + ".mp3");

                //Delete files
                File.Delete(workingDir + "\\" + filenameWithExtension);
                File.Delete(workingDir + "\\" + audioFileName);
                foreach (var file in Directory.GetFiles(workingDir))
                {
                    if (file.EndsWith(".srt"))
                    {
                        File.Delete(file);
                    }
                }
            }

            if (isVideoExist)
            {
                // delete video
                // log error
                return true;
            }
 
            return true;
        }

        private void UploadFileToS3(string videoFileName, string audioFileName, string localName, string savedName)
        {
            var client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast2);

            try
            {
                PutObjectRequest putRequest = new PutObjectRequest
                                                  {
                                                      BucketName = "aptlink/public/" + localName,
                                                      Key = savedName,
                                                      FilePath = Directory.GetCurrentDirectory() + "\\" + audioFileName,
                                                      CannedACL = S3CannedACL.PublicRead
                };

                PutObjectResponse response = client.PutObject(putRequest);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null
                    && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                        || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }
            catch (Exception e)
            {
                
            }
        }
    }
}
