using Mono.Data.SqliteClient;
using System.Collections;
using System.IO;
using System;


// All kinds of database items subclass from this.
public class DbItem {
	uint id;
	public uint Id {
		get {
			return id;
		}
	}

	protected DbItem (uint id) {
		this.id = id;
	}
}

public class DbItemEventArgs {
	private DbItem [] items;
	
	public DbItem [] Items {
		get { return items; }
	}
	
	public DbItemEventArgs (DbItem [] items)
	{
		this.items = items;
	}
	
	public DbItemEventArgs (DbItem item)
	{
		this.items = new DbItem [] { item };
	}
}

// A Store maps to a SQL table.  We have separate stores (i.e. SQL tables) for tags, photos and imports.

public delegate void ItemsAddedHandler (object sender, DbItemEventArgs args);
public delegate void ItemsRemovedHandler (object sender, DbItemEventArgs args);
public delegate void ItemsChangedHandler (object sender, DbItemEventArgs args);

public abstract class DbStore {
	// DbItem cache.

	public event ItemsAddedHandler   ItemsAdded;
	public event ItemsRemovedHandler ItemsRemoved;
	public event ItemsChangedHandler ItemsChanged;

	protected Hashtable item_cache;
	bool cache_is_immortal;

	protected void AddToCache (DbItem item)
	{
		if (item_cache.Contains (item.Id))
			item_cache.Remove (item.Id);

		if (cache_is_immortal)
			item_cache.Add (item.Id, item);
		else
			item_cache.Add (item.Id, new WeakReference (item));
	}

	protected DbItem LookupInCache (uint id)
	{
		if (cache_is_immortal)
			return item_cache [id] as DbItem;

		WeakReference weakref = item_cache [id] as WeakReference;
		if (weakref == null)
			return null;
		else
			return (DbItem) weakref.Target;
	}

	protected void RemoveFromCache (DbItem item)
	{
		item_cache.Remove (item.Id);
	}

	protected void EmitAdded (DbItem item)
	{
		EmitAdded (new DbItem [] { item });
	}

	protected void EmitAdded (DbItem [] items)
	{
		if (ItemsAdded != null)
			ItemsAdded (this, new DbItemEventArgs (items));
	}

	protected void EmitChanged (DbItem item)
	{
		EmitChanged (new DbItem [] { item });
	}

	protected void EmitChanged (DbItem [] items)
	{
		EmitChanged (items, new DbItemEventArgs (items));
	}

	protected void EmitChanged (DbItem [] items, DbItemEventArgs args)
	{
		if (ItemsChanged != null)
			ItemsChanged (this, args);
	}

	protected void EmitRemoved (DbItem item)
	{
		EmitRemoved (new DbItem [] { item });
	}

	protected void EmitRemoved (DbItem [] items)
	{
		if (ItemsRemoved != null)
			ItemsRemoved (this, new DbItemEventArgs (items));
	}

	public bool CacheEmpty {
		get {
			return item_cache.Count == 0;
		}
	}

	// Sqlite stuff.

	SqliteConnection connection;
	protected SqliteConnection Connection {
		get {
			return connection;
		}
	}


	// Constructor.

	public DbStore (SqliteConnection connection,
			bool cache_is_immortal)
	{
		this.connection = connection;
		this.cache_is_immortal = cache_is_immortal;

		item_cache = new Hashtable ();
	}


	// Abstract methods.

	public abstract DbItem Get (uint id);
	public abstract void Remove (DbItem item);
	// If you have made changes to "obj", you have to invoke Commit() to have the changes
	// saved into the database.
	public abstract void Commit (DbItem item);


	// Utility methods.

	// Subclasses use this to generate valid SQL commands.
	protected static string SqlString (string s) {
		return s.Replace ("'", "''");
	}

	public void BeginTransaction () {
		SqliteCommand command = new SqliteCommand ();
		command.Connection = Connection;
		command.CommandText = "BEGIN TRANSACTION";
		command.ExecuteScalar ();
		command.Dispose ();
	}
	
	public void CommitTransaction () {
		SqliteCommand command = new SqliteCommand ();
		command.Connection = Connection;
		command.CommandText = "COMMIT TRANSACTION";
		command.ExecuteScalar ();
		command.Dispose ();
	}
	
	public void RollbackTransaction () {
		SqliteCommand command = new SqliteCommand ();
		command.Connection = Connection;
		command.CommandText = "ROLLBACK";
		command.ExecuteScalar ();
		command.Dispose ();
	}

}


// The Database puts the stores together.

public class Db : IDisposable {

	TagStore tag_store;
	PhotoStore photo_store;
 	ImportStore import_store;
	bool empty;

	public TagStore Tags {
		get {
			return tag_store;
		}
	}

	public ImportStore Imports {
		get {
			return import_store;
		}
	}

	public PhotoStore Photos {
		get {
			return photo_store;
		}
	}

	public bool Sync {
		set {
			SqliteCommand command = new SqliteCommand ();
			command.Connection = sqlite_connection;
			command.CommandText = "PRAGMA synchronous = " + (value ? "ON" : "OFF");
			command.ExecuteScalar ();
			command.Dispose ();
		}
	}

	public void BeginTransaction () {
		SqliteCommand command = new SqliteCommand ();
		command.Connection = sqlite_connection;
		command.CommandText = "BEGIN TRANSACTION";
		command.ExecuteScalar ();
		command.Dispose ();
	}
	
	public void CommitTransaction () {
		SqliteCommand command = new SqliteCommand ();
		command.Connection = sqlite_connection;
		command.CommandText = "COMMIT TRANSACTION";
		command.ExecuteScalar ();
		command.Dispose ();
	}
	
	public void RollbackTransaction () {
		SqliteCommand command = new SqliteCommand ();
		command.Connection = sqlite_connection;
		command.CommandText = "ROLLBACK";
		command.ExecuteScalar ();
		command.Dispose ();
	}

	SqliteConnection sqlite_connection;


	// Constructor.

	public Db (string path, bool create_if_missing)
	{
		bool new_db = ! File.Exists (path);

		if (new_db && ! create_if_missing)
			throw new Exception (path + ": File not found");

		sqlite_connection = new SqliteConnection ();
		sqlite_connection.ConnectionString = "URI=file:" + path;

		sqlite_connection.Open ();

		tag_store = new TagStore (sqlite_connection, new_db);
		import_store = new ImportStore (sqlite_connection, new_db);
 		photo_store = new PhotoStore (sqlite_connection, new_db, tag_store);

		empty = new_db;
	}

	public bool Empty {
		get {
			return empty;
		}
	}

	public void Dispose () {}
}


public class DbUtils {
	public static DateTime DateTimeFromUnixTime (uint unix_time)
	{
		DateTime date_time = new DateTime (1970, 1, 1).ToLocalTime ();
		return date_time.AddSeconds (unix_time);
	}

	public static uint UnixTimeFromDateTime (DateTime date_time)
	{
		return (uint) (date_time - new DateTime (1970, 1, 1).ToLocalTime ()).TotalSeconds;
	}
}
