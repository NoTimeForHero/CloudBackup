// File: UploadJobState.cs
// Created by NoTimeForHero, 2022
// Distributed under the Apache License 2.0

using System;
using System.IO;

namespace CloudBackuper
{
    public class UploadJobState
    {
        public string status { get; private set; }
        public bool inProgress { get; private set; }
        public event Action<UploadJobState> onUpdate;

        public bool isBytes { get; set; }
        public long current { get; private set; }
        public long total { get; private set; }

        public void Update(string status, bool raiseEvent = true)
        {
            inProgress = true;
            this.status = status;
            if (raiseEvent) onUpdate?.Invoke(this);
        }

        public void Progress(long current, long total)
        {
            this.current = current;
            this.total = total;
        }

        public void Done(string status)
        {
            this.status = status;
            inProgress = false;
            isBytes = false;
            current = 0;
            total = 0;
            onUpdate?.Invoke(this);
        }

        // На будущее если понадобится улучшить производительности WebSocket
        public byte[] Serialize()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(inProgress);
            writer.Write(isBytes);
            writer.Write(current);
            writer.Write(total);
            writer.Write(status.Length);
            writer.Write(status);
            writer.Flush();
            stream.Flush();
            return stream.ToArray();
        }
    }
}