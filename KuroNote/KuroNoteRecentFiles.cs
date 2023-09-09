using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KuroNote
{
    /// <summary>
    /// recent.json is converted into an instance of KuroNoteRecentFiles using JSON.NET
    /// </summary>
    class KuroNoteRecentFiles
    {
        //constants
        private const int RECENT_FILES_MAX_CAPACITY = 10;

        //meta
        private Log log;

        //attributes with default values
        public Queue<string> recentFiles = new Queue<string>();

        public KuroNoteRecentFiles(Log _mainLog)
        {
            log = _mainLog;
        }

        /// <summary>
        /// Update this object according to the settings stored in recent.json
        /// </summary>
        public void retrieveRecentFiles()
        {
            try {
                using (StreamReader sr = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\KuroNote\\recent.json"))
                {
                    string json = sr.ReadToEnd();
                    KuroNoteRecentFiles knrfFile = JsonConvert.DeserializeObject<KuroNoteRecentFiles>(json);

                    this.recentFiles = knrfFile.recentFiles;
                }
            } catch (Exception e) {
                log.addLog("Error during retrieveRecentFiles: " + e.ToString());
                log.addLog("Creating new recent.json!");
                updateRecentFiles(); //Creates a new recent.json file with default values (since the values weren't changed from the defaults)
            }
        }

        /// <summary>
        /// Adds the specified file name to Recent files, a FIFO queue with a capacity of 10
        /// (if the specified file name already exists in the queue, the existing (old) entry is removed)
        /// </summary>
        /// <param name="fileToBeAdded">The file name to add to Recent Files</param>
        public void addRecentFile(string fileToBeAdded)
        {
            //if this file is already in Recent Files, remove it so that it can be re-added on top
            purgeDuplicates(fileToBeAdded);

            //if Recent Files is full, remove the oldest element
            if(this.recentFiles.Count == RECENT_FILES_MAX_CAPACITY) {
                log.addLog("Removed oldest entry \"" + this.recentFiles.Peek() + "\" from Recent Files");
                this.recentFiles.Dequeue();
            }

            //add the new Recent File on top
            this.recentFiles.Enqueue(fileToBeAdded);
            log.addLog("Added \"" + fileToBeAdded + "\" to Recent Files (count: " + this.recentFiles.Count + ")");
            updateRecentFiles();
        }

        /// <summary>
        /// Deletes the specified file name from Recent files, a FIFO queue with a capacity of 10
        /// This method is not part of the normal workflow for recent files,
        /// It only exists to remove broken links (i.e., the user clicks a recent file but it doesn't successfully open)
        /// </summary>
        /// <param name="fileToBeRemoved"></param>
        public void deleteRecentFile(string fileToBeRemoved)
        {
            //deletes the specified Recent File by replacing the queue with a copy of queue that omits the specified files
            this.recentFiles = new Queue<string>(this.recentFiles.Where(x => x != fileToBeRemoved));
            log.addLog("Removed \"" + fileToBeRemoved + "\" from Recent Files (count: " + this.recentFiles.Count + ")");
            updateRecentFiles();
        }

        /// <summary>
        /// adityamutharia's technique to remove duplicates from a queue
        /// (https://www.geeksforgeeks.org/how-to-remove-a-specific-element-from-queue/)
        /// If fileToBeAdded matches an existing Recent File,
        /// removes the single oldest occurance of Recent File
        /// NOTE: This method only removes single duplications
        /// </summary>
        /// <param name="fileToBeAdded">The file name to check for duplicates of</param>
        private void purgeDuplicates(string fileToBeAdded)
        {
            Queue<string> tempQ = new Queue<string>();
            int originalQSize = this.recentFiles.Count;
            int numberOfNonDupes = 0; //number of elements preceding the duplicate element

            //Search for duplicates
            while(this.recentFiles.Count != 0 && !this.recentFiles.Peek().Equals(fileToBeAdded)) {
                tempQ.Enqueue(this.recentFiles.Peek());
                this.recentFiles.Dequeue();
                numberOfNonDupes++;
            }

            if(this.recentFiles.Count == 0)
            {
                //no duplicates found
                log.addLog("No duplicates found in Recent Files");
                while(tempQ.Count != 0) {
                    //restore original queue
                    this.recentFiles.Enqueue(tempQ.Peek());
                    tempQ.Dequeue();
                }
            } else {
                //duplicates found
                this.recentFiles.Dequeue(); //remove duplicate
                log.addLog("Duplicate found and removed from Recent Files");

                while (tempQ.Count != 0) {
                    //restore original queue
                    this.recentFiles.Enqueue(tempQ.Peek());
                    tempQ.Dequeue();
                }

                int numberOfElementsToPush = originalQSize - numberOfNonDupes - 1;
                //push elements from the front to back
                while(numberOfElementsToPush-- > 0) {
                    string tempPeek = this.recentFiles.Peek();
                    this.recentFiles.Dequeue();
                    this.recentFiles.Enqueue(tempPeek);
                }
            }
        }

        /// <summary>
        /// Deletes all recent files names
        /// </summary>
        public void clearRecentFiles()
        {
            this.recentFiles.Clear();
            updateRecentFiles();
            log.addLog("Recent Files cleared and updated");
        }

        /// <summary>
        /// Update recent.json according to the settings stored in this object
        /// </summary>
        public void updateRecentFiles()
        {
            try {
                using (StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\KuroNote\\recent.json"))
                {
                    string json = JsonConvert.SerializeObject(this);

                    sw.Write(json);
                }
            } catch (Exception e) {
                log.addLog("Error during updateRecentFiles: " + e.ToString());
            }
        }
    }
}
