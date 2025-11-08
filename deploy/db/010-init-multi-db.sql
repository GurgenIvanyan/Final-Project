DO
$$
BEGIN
   IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'Playlist.Api') THEN
      PERFORM dblink_exec('dbname=' || current_database(), 'CREATE DATABASE "Playlist_Api"');
   END IF;
   IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'UserServiceDb') THEN
      PERFORM dblink_exec('dbname=' || current_database(), 'CREATE DATABASE "UserServiceDb"');
   END IF;
END
$$ LANGUAGE plpgsql;
