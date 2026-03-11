CREATE TABLE IF NOT EXISTS apps (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    version TEXT,
    publisher TEXT,
    architecture TEXT,
    install_date TEXT,
    install_path TEXT,
    source TEXT
);

CREATE TABLE IF NOT EXISTS dependencies (
    app_id TEXT NOT NULL,
    dll_name TEXT NOT NULL,
    resolved_path TEXT,
    is_missing INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (app_id, dll_name),
    FOREIGN KEY (app_id) REFERENCES apps(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS frameworks (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    version TEXT,
    description TEXT,
    rules_ref TEXT
);

CREATE TABLE IF NOT EXISTS app_frameworks (
    app_id TEXT NOT NULL,
    framework_id TEXT NOT NULL,
    PRIMARY KEY (app_id, framework_id),
    FOREIGN KEY (app_id) REFERENCES apps(id) ON DELETE CASCADE,
    FOREIGN KEY (framework_id) REFERENCES frameworks(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS snapshots (
    id TEXT PRIMARY KEY,
    taken_at TEXT NOT NULL,
    label TEXT
);

CREATE TABLE IF NOT EXISTS snapshot_apps (
    snapshot_id TEXT NOT NULL,
    app_id TEXT NOT NULL,
    state_json TEXT NOT NULL,
    PRIMARY KEY (snapshot_id, app_id),
    FOREIGN KEY (snapshot_id) REFERENCES snapshots(id) ON DELETE CASCADE,
    FOREIGN KEY (app_id) REFERENCES apps(id) ON DELETE CASCADE
);

-- FTS5 virtual table for searching apps
CREATE VIRTUAL TABLE IF NOT EXISTS apps_fts USING fts5(
    name,
    publisher,
    content='apps'
);

-- Triggers to keep FTS table in sync
CREATE TRIGGER IF NOT EXISTS apps_ai AFTER INSERT ON apps BEGIN
  INSERT INTO apps_fts(rowid, name, publisher) VALUES (new.rowid, new.name, new.publisher);
END;
CREATE TRIGGER IF NOT EXISTS apps_ad AFTER DELETE ON apps BEGIN
  INSERT INTO apps_fts(apps_fts, rowid, name, publisher) VALUES('delete', old.rowid, old.name, old.publisher);
END;
CREATE TRIGGER IF NOT EXISTS apps_au AFTER UPDATE ON apps BEGIN
  INSERT INTO apps_fts(apps_fts, rowid, name, publisher) VALUES('delete', old.rowid, old.name, old.publisher);
  INSERT INTO apps_fts(rowid, name, publisher) VALUES (new.rowid, new.name, new.publisher);
END;
