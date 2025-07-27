using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Magic
{
    public static class HelperNET
    {
        delegate void RunCrossThreadMethodCallBack(Control control, Action myMethod);

        public static Form FindMainForm(Control control)
        {
            Form? mainForm = control.FindForm();

            while (mainForm != null && mainForm.GetType() != typeof(Form))
            {
                mainForm = mainForm.ParentForm;
            }

            return mainForm!;
        } // end of method

        public static bool IsUrlValid(string url)
        {
            // Mencoba membuat instance Uri dengan URL yang diberikan
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? result))
            {
                // Jika berhasil, dan protokol adalah http atau https, maka URL valid
                return result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps;
            }
            else
            {
                // Jika gagal membuat instance Uri, atau protokol bukan http atau https, maka URL tidak valid
                return false;
            }
        } // end of method

        public static void ShowErrorMessages(List<string> errorMessages, Control control)
        {
            StringBuilder errorMessage = new StringBuilder();

            errorMessage.Append("Input tidak valid. Silakan cek kembali hal di bawah ini : ");
            errorMessage.Append(Environment.NewLine);

            foreach (string single in errorMessages)
            {
                errorMessage.Append($"● {single}");
                errorMessage.Append(Environment.NewLine);
            }

            RunCrossThreadMethod(control, () =>
            {
                MessageBox.Show(control, errorMessage.ToString(), "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            });

            return;
        } // end of method

        public static void RunCrossThreadMethod(Control control, Action myMethod)
        {

            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (control.InvokeRequired)
            {
                //control.BeginInvoke(new Action(() => RunCrossThreadMethod(control, myMethod)));
                control.Invoke(new Action(() => RunCrossThreadMethod(control, myMethod)));
            }
            else
            {
                myMethod();
            }

        } // end of function

        public static void RunCrossThreadMethod_(Control control, Action myMethod)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (control.InvokeRequired)
            {
                RunCrossThreadMethodCallBack d = new RunCrossThreadMethodCallBack(RunCrossThreadMethod);
                control.Invoke(d, new object[] { control, myMethod });
            }
            else
            {
                myMethod();
            }
        } // end of function

        public static object GetCellValueByColumnName(this DataGridViewCellCollection CellCollection, string dataPropertyName)
        {
            return CellCollection.Cast<DataGridViewCell>().First(c => c.OwningColumn.DataPropertyName == dataPropertyName).Value;
        } // end of method

        public static string Spintax_old(string input)
        {
            Random rnd = new Random();
            // Loop over string until all patterns exhausted.
            string pattern = "{[^{}]*}";
            //string pattern = "{^{}*}";
            Match m = Regex.Match(input, pattern);
            while (m.Success)
            {
                // Get random choice and replace pattern match.
                string seg = input.Substring(m.Index + 1, m.Length - 2);
                string[] choices = seg.Split('|');
                input = input.Substring(0, m.Index) + choices[rnd.Next(choices.Length)] + input.Substring(m.Index + m.Length);
                m = Regex.Match(input, pattern);
            }

            // Return the modified string.
            return input;
        } // end of method

        private static readonly Random rnd = new Random();

        public static string Spintax(string input)
        {

            // Ganti tab dengan spasi
            input = input.Replace("\t", " ");

            // Hapus spasi setelah `{`
            input = Regex.Replace(input, @"{\s+", "{");

            // Hapus spasi sebelum `}`
            input = Regex.Replace(input, @"\s+}", "}");

            // Hapus spasi di sekitar `|`
            input = Regex.Replace(input, @"\s*\|\s*", "|");

            string pattern = "{[^{}]*}";
            Match m = Regex.Match(input, pattern);

            Random rnd = new Random(); // Pindahkan di sini jika tidak punya variable rnd global

            while (m.Success)
            {
                string seg = input.Substring(m.Index + 1, m.Length - 2);

                // Split dan buang slot kosong
                string[] choices = seg
                    .Split('|')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();

                // Jika semua slot kosong, replacement = ""
                string replacement = choices.Length > 0
                    ? choices[rnd.Next(choices.Length)]
                    : "";

                input = input.Substring(0, m.Index) + replacement + input.Substring(m.Index + m.Length);
                m = Regex.Match(input, pattern);
            }

            // Hilangkan spasi berlebih, trim di awal/akhir
            return Regex.Replace(input, @"\s{2,}", " ").Trim();

        } // end of method

        public static string CreateRandomString(int length, bool onlyNumbers = false)
        {
            Random random = new Random();

            string chars = "";

            if (onlyNumbers)
            {
                chars = "1234567890";
            }
            else
            {
                chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
            }

            string newKey = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());

            return newKey;
        } // end of function

        public static string CreateRandomAlphabeticLowerCaseString(int length)
        {
            Random random = new Random();

            string chars = "abcdefghijklmnopqrstuvwxyz";
            string newKey = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());

            return newKey;
        } // end of method

        public static string StripNonAlphanumeric(this string input)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            input = rgx.Replace(input, "");

            return input;
        } // end of method

        public static string ToTitleCase(this string str)
        {
            string titleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
            return titleCase;
        } // end of method

        public static int NowUnixLocal()
        {

            // Dapatkan waktu lokal saat ini
            DateTimeOffset localDateTimeOffset = DateTimeOffset.Now;

            // Konversi ke Unix timestamp lokal
            long unixTimestampLocal = localDateTimeOffset.ToUnixTimeSeconds();

            return Convert.ToInt32(unixTimestampLocal);

        } // end of function

        public static HasChanged HasDayChanged(string pastDateTimeString)
        {

            HasChanged hasChanged = new HasChanged();

            DateTime pastDateTime = DateTime.ParseExact(pastDateTimeString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            // Dapatkan DateTime lokal saat ini
            DateTime currentLocalDateTime = DateTime.Now;
            hasChanged.CurrentDateTime = currentLocalDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            // Bandingkan Year, Month, dan Day
            hasChanged.Changed = pastDateTime.Year != currentLocalDateTime.Year ||
                pastDateTime.Month != currentLocalDateTime.Month ||
                pastDateTime.Day != currentLocalDateTime.Day;

            return hasChanged;

        } // end of method

        public static HasChanged HasHourChanged(string pastDateTimeString)
        {

            HasChanged hasChanged = new HasChanged();

            DateTime pastDateTime = DateTime.ParseExact(pastDateTimeString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            // Dapatkan DateTime lokal saat ini
            DateTime currentLocalDateTime = DateTime.Now;

            hasChanged.CurrentDateTime = currentLocalDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            // Bandingkan Year, Month, Day, dan Hour
            hasChanged.Changed = pastDateTime.Year != currentLocalDateTime.Year ||
                pastDateTime.Month != currentLocalDateTime.Month ||
                pastDateTime.Day != currentLocalDateTime.Day ||
                pastDateTime.Hour != currentLocalDateTime.Hour;

            return hasChanged;

        } // end of method

        public class HasChanged
        {

            public bool Changed { get; set; }
            public string? CurrentDateTime { get; set; }

        } // end of class

        public static void PutContentToClipboard(string content)
        {
            Thread t = new Thread(() => {
                try
                {
                    Clipboard.SetText(content);
                }
                catch
                {
                    Clipboard.Clear();
                }
            });

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                t.SetApartmentState(ApartmentState.STA);
            }

            t.Start();
            t.Join();
        } // end of method

        public static int RandomNumberBetween(int from, int to)
        {
            if (from > to)
            {
                return from;
            }

            Random r = new Random();

            int index = r.Next(from, to + 1);

            return index;
        } // end of method

        public static string ApplicationName()
        {

            // Mendapatkan Assembly saat ini (proyek utama)
            Assembly? currentAssembly = Assembly.GetEntryAssembly();

            // Mendapatkan nama aplikasi dari proyek saat ini
            string applicationName = string.Empty;

            if (currentAssembly != null)
            {
                applicationName = currentAssembly.GetName().Name ?? "N/A";
            }

            return applicationName;

        } // end of method

        public static string GetMasterDataFolder(string AppName = "")
        {

            string appName = "";

            if(AppName == "")
            {
                appName = ApplicationName();
            }
            else
            {
                appName = AppName;
            }

            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string masterDataFolder = Path.Combine(appDataDir, appName);

            if (!Directory.Exists(masterDataFolder))
            {
                Directory.CreateDirectory(masterDataFolder);
            }

            return masterDataFolder;

        } // end of method

        public static string GetBinFolder()
        {
            string? location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string? directoryName = System.IO.Path.GetDirectoryName(location);

            if (directoryName != null)
            {
                return directoryName;
            }
            else
            {
                return string.Empty;
            }
        } // end of method

        public static bool IsNewDay(long lastReset = 0)
        {
            DateTimeOffset saatIni = DateTimeOffset.Now;

            // jadikan ke unix timestamp untuk digunakan nanti
            int saatIniUnix = Magic.HelperNET.NowUnixLocal(); // Local time

            DateTimeOffset lastResetDateTime = new DateTimeOffset();

            if (lastReset != 0)
            {
                // option ditemukan, set lastReset dari unixtimestamp option
                lastResetDateTime = DateTimeOffset.FromUnixTimeSeconds(lastReset).DateTime;
            }
            else
            {
                // option tidak ditemukan, set lastReset ke minggu lalu
                lastResetDateTime = DateTimeOffset.Now.AddDays(-7);
            }

            int lastResetDay = lastResetDateTime.Date.Day;
            int toDay = saatIni.Date.Day;

            int selisihTanggal = toDay - lastResetDay;

            // tanggal sudah ganti
            if (selisihTanggal != 0)
            {
                return true;
            }

            return false;

        } // end of method

        public class ListItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            //public bool Selected { get; set; }

            public ListItem(string text, int value, bool selected = false)
            {
                Text = text;
                Value = value;
                //Selected = selected;
            } // end of method

        } // end of class

        public class IdAndNamePair
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        } // end of method

        public class UidAndNamePair
        {
            public int Uid { get; set; }
            public string? Name { get; set; }
        } // end of method


        // Digunakan untuk memberikan waktu minimum untuk sebuah proses

        private static DateTime? startTime;
        private static bool processStarted = false;

        public static void StartMinimumTime()
        {
            startTime = DateTime.Now;
            processStarted = true;
        } // end of method

        public static void EndMinimumTime(int minimumDurationMilliseconds)
        {
            if (!processStarted)
            {
                Console.WriteLine("Error: Process has not been started.");
                return;
            }

            if (startTime == null)
            {
                Console.WriteLine("Error: Start time is not set.");
                return;
            }

            // Hitung waktu yang telah berlalu untuk proses
            TimeSpan elapsedTime = DateTime.Now - startTime.Value;

            // Tentukan apakah perlu menunggu tambahan atau tidak
            int remainingTime = minimumDurationMilliseconds - (int)elapsedTime.TotalMilliseconds;

            if (remainingTime > 0)
            {
                Thread.Sleep(remainingTime);
            }
        } // end of method

        public static void Shuffle<T>(List<T> list)
        {

            Random random = new Random();

            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                // Tukar elemen
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }

        } // end of method

        public static string HariIni(string dayShort)
        {
            return dayShort switch
            {
                "Sun" => "Minggu",
                "Mon" => "Senin",
                "Tue" => "Selasa",
                "Wed" => "Rabu",
                "Thu" => "Kamis",
                "Fri" => "Jumat",
                "Sat" => "Sabtu",
                _ => dayShort
            };
        } // end of method

        public static string BulanIni(string monthNum)
        {
            return monthNum switch
            {
                "01" => "Januari",
                "02" => "Februari",
                "03" => "Maret",
                "04" => "April",
                "05" => "Mei",
                "06" => "Juni",
                "07" => "Juli",
                "08" => "Agustus",
                "09" => "September",
                "10" => "Oktober",
                "11" => "November",
                "12" => "Desember",
                _ => monthNum
            };
        } // end of method

        public static string Rupiah(decimal value)
        {

            return string.Format(new System.Globalization.CultureInfo("id-ID"), "Rp {0:N0}", value);

        } // end of method

        public static string ConvertMessageShortcodes(string message, Dictionary<string, object?> shortcodes)
        {

            DateTime plusEnamJam = DateTime.UtcNow.AddHours(13); // UTC + 7 + 6

            shortcodes["time_max"] = plusEnamJam.ToString("H:mm"); // sama seperti G:i di PHP
            shortcodes["date_max"] = plusEnamJam.ToString("dd");
            shortcodes["year_max"] = plusEnamJam.ToString("yyyy");
            shortcodes["day_max"] = HariIni(plusEnamJam.ToString("ddd")); // misalnya "Sun"
            shortcodes["month_max"] = BulanIni(plusEnamJam.ToString("MM"));

            foreach (var kvp in shortcodes)
            {
                string key = kvp.Key;
                object? value = kvp.Value ?? "";

                if (key == "price" || key == "value")
                {
                    if (decimal.TryParse(value.ToString(), out decimal price))
                    {
                        value = Rupiah(price);
                    }
                }

                message = message.Replace($"[{key}]", value.ToString());
            }

            return message;

        } // end of method

    } // end of class Helper
} // end of namespace
