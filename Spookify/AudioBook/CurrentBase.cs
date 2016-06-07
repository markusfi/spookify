using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Spookify
{
	[Serializable]
	public class CurrentBase<TClass>  where  TClass : CurrentBase<TClass>, new()
	{
		protected static TClass Prototype = new TClass();
		protected static TClass _currentState;
		public virtual string Filename()  { return typeof(TClass).Name; } 

		public string StateFilename {
			get {
				var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
				var filename = Path.Combine (documents, Filename());
				return filename;
			}
		}
		public string StateFilenameTemp {
			get {
				return StateFilename + ".tmp";
			}
		}
		public string StateFilenameBackup {
			get {
				return StateFilename + ".backup";
			}
		}
		public static TClass Current
		{ 
			get {
				if (_currentState == null) {
					lock (typeof(CurrentState)) {
						BinaryFormatter formatter = new BinaryFormatter ();
						try {
							if (File.Exists (Prototype.StateFilename)) {
								using (var fs = new FileStream (Prototype.StateFilename, FileMode.Open, FileAccess.Read)) {
									_currentState = formatter.Deserialize (fs) as TClass;
								}
							} else if (File.Exists (Prototype.StateFilenameBackup)) {
								using (var fs = new FileStream (Prototype.StateFilenameBackup, FileMode.Open, FileAccess.Read)) {
									_currentState = formatter.Deserialize (fs) as TClass;
								}
							}
						} catch {
							if (File.Exists (Prototype.StateFilenameBackup)) {
								// Try the Backup file  
								try {
									using (var fs = new FileStream (Prototype.StateFilenameBackup, FileMode.Open, FileAccess.Read)) {
										_currentState = formatter.Deserialize (fs) as TClass;
									}
								} catch {
									// Backup did not work either...
								}
							}
						}
						if (_currentState == null)
							_currentState = new TClass ();
					}
				}
				return _currentState;			
			} 
		}
		public virtual void StoreCurrent()
		{
			#if DEBUG
			Console.WriteLine("Store File: "+Prototype.StateFilename);
			#endif
			lock (typeof(CurrentState)) {
				if (File.Exists (Prototype.StateFilenameTemp))
					File.Delete (Prototype.StateFilenameTemp);

				BinaryFormatter formatter = new BinaryFormatter ();
				using (var fs = new FileStream (Prototype.StateFilenameTemp, FileMode.CreateNew, FileAccess.ReadWrite)) {
					fs.Seek (0, SeekOrigin.Begin);
					fs.SetLength (0);
					formatter.Serialize (fs, this);
				}
				if (File.Exists (Prototype.StateFilenameBackup))
					File.Delete (Prototype.StateFilenameBackup);
				if (File.Exists (Prototype.StateFilename))
					File.Replace (Prototype.StateFilenameTemp, Prototype.StateFilename, Prototype.StateFilenameBackup);
				else
					File.Move (Prototype.StateFilenameTemp, Prototype.StateFilename);
			}
		}
	}
}

