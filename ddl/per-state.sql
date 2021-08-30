BEGIN;

-- Creates participant records tables and their access controls.
-- Assumes 4 database roles, to be set via the psql -v option:
--  * cluster `superuser`
--  * database `owner`, which owns the tables, sequences
--  * database `admin`, which gets read/write access
--  * database `reader`, which gets read-only access

SET search_path=piipan,public;

CREATE TABLE IF NOT EXISTS uploads(
	id serial PRIMARY KEY,
	created_at timestamp NOT NULL,
	publisher text NOT NULL
);

COMMENT ON TABLE uploads IS 'Bulk PII upload events';
COMMENT ON COLUMN uploads.created_at IS 'Date/time the records were uploaded in bulk';
COMMENT ON COLUMN uploads.publisher IS 'User or service account that performed the upload';

CREATE TABLE IF NOT EXISTS participants(
	id serial PRIMARY KEY,
	lds_hash text NOT NULL
		CONSTRAINT hex_check CHECK (lds_hash ~ '^[0-9a-f]{128}$'),
	upload_id integer REFERENCES uploads (id),
    	case_id text NOT NULL,
    	participant_id text NOT NULL,
	benefits_end_date date,
    	recent_benefit_months date[],
    	protect_location boolean
);

COMMENT ON TABLE participants IS 'Program participant';
COMMENT ON COLUMN participants.lds_hash IS 'Participant''s deidentified data as hex value';
COMMENT ON COLUMN participants.case_id IS 'Participant''s state-specific case identifier';
COMMENT ON COLUMN participants.participant_id IS 'Participant''s state-specific identifier';
COMMENT ON COLUMN participants.benefits_end_date IS 'Participant''s ending benefits date';
COMMENT ON COLUMN participants.recent_benefit_months IS 'Participant''s recent benefit months';
COMMENT ON COLUMN participants.protect_location IS 'Participant''s vulnerability status';

CREATE INDEX IF NOT EXISTS participants_lds_hash_idx ON participants (lds_hash, upload_id);
CREATE UNIQUE INDEX IF NOT EXISTS participants_uniq_ids_idx ON participants (case_id, participant_id, upload_id);

-- "superuser" account under Azure is not so super
GRANT :owner to :superuser;

ALTER TABLE uploads OWNER TO :owner;
ALTER TABLE participants OWNER TO :owner;

ALTER SEQUENCE participants_id_seq OWNER TO :owner;
ALTER SEQUENCE uploads_id_seq OWNER TO :owner;

GRANT SELECT, INSERT, UPDATE, DELETE ON participants TO :admin;
GRANT SELECT, INSERT, UPDATE, DELETE ON uploads TO :admin;
GRANT USAGE, SELECT, UPDATE ON participants_id_seq TO :admin;
GRANT USAGE, SELECT, UPDATE ON uploads_id_seq TO :admin;

GRANT SELECT ON participants TO :reader;
GRANT SELECT ON uploads TO :reader;

-- restore privileges
REVOKE :owner FROM :superuser;

COMMIT;
