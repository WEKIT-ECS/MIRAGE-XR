namespace MirageXR
{
    public class SessionContainer
    {
        private const long KB = 1024;
        private const long MB = KB * KB;
        private const long GB = KB * KB * KB;

        private const string LESS_THAN_KB = "< 1KB";

        private const string KB_FORMAT = "{0:F2}KB";
        private const string MB_FORMAT = "{0:F2}MB";
        private const string GB_FORMAT = "{0:F2}GB";
        private const string ZIP = ".zip";

        public Session Session { get; set; }

        public Activity Activity { get; set; }

        public string Filesize
        {
            get
            {
                if (Session == null)
                {
                    return string.Empty;
                }

                long size = Session.filesize;
                string convertedSize;

                if (size > GB)
                {
                    convertedSize = string.Format(GB_FORMAT, size / (float)GB);
                }
                else if (size > MB)
                {
                    convertedSize = string.Format(MB_FORMAT, size / (float)MB);
                }
                else if (size > KB)
                {
                    convertedSize = string.Format(KB_FORMAT, size / (float)KB);
                }
                else
                {
                    convertedSize = LESS_THAN_KB;
                }

                return convertedSize;
            }
        }

        public string FileIdentifier
        {
            get
            {
                if (Activity != null)
                {
                    return Activity.id;
                }

                if (Session != null)
                {
                    return Session.sessionid;
                }

                return string.Empty;
            }
        }

        public string Name
        {
            get
            {
                if (Activity != null)
                {
                    return Activity.name;
                }

                if (Session != null)
                {
                    return string.IsNullOrEmpty(Session.title)
                        ? Session.filename.Replace(ZIP, string.Empty) : Session.title;
                }

                return string.Empty;
            }
        }


        public string deadline
        {
            get
            {
                if (Session != null && Session.deadline != null)
                {
                    return $"Deadline: {Session.deadline}";
                }
                return "Not enrolled to this course.";
            }
        }

        public bool hasDeadline
        {
            get
            {
                if (Session != null && Session.deadline != null)
                {
                    return true;
                }
                return false;
            }
        }

        public string author
        {
            get
            {
                if (Session != null && Session.author != null)
                {
                    return $"Author: {Session.author}";
                }
                return "Author: You";
            }
        }


        public bool userIsOwner
        {
            get
            {
                if (DBManager.LoggedIn && Session != null && Session.userid == DBManager.userid)
                {
                    return true;
                }
                return false;
            }
        }


        public string ItemID
        {
            get
            {
                if (Session != null && Session.itemid != null)
                {
                    return Session.itemid;
                }
                return null;
            }
        }

        public string AbsoluteURL
        {
            get
            {
                if (Session != null)
                {
                    return $"{DBManager.domain}/pluginfile.php/{Session.contextid}/{Session.component}/{Session.filearea}/{Session.itemid}/{Session.filename}";
                }
                return null;
            }
        }

        public bool ExistsRemotely { get => Session != null; }
        public bool ExistsLocally { get => Activity != null; }
        public bool IsEditable { get; set; }

        public bool IsDownloading { get; set; }

        public bool HasError { get; set; }
    }
}