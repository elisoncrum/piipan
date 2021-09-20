BEGIN;

CREATE TYPE hash_type AS ENUM ('ldshash');
CREATE TYPE status AS ENUM ('open', 'closed');

CREATE TABLE IF NOT EXISTS matches(
    id serial PRIMARY KEY,
    match_id text UNIQUE NOT NULL,
    created_at timestamp NOT NULL,
    initator text NOT NULL,
    hash text NOT NULL,
    hash_type hash_type NOT NULL default 'ldshash';
    input jsonb,
    data jsonb NOT NULL,
    invalid bool NOT NULL default FALSE,
    status status NOT NULL default 'open'
);

COMMENT ON TABLE matches IS 'Match records';
COMMENT ON COLUMN matches.match_id IS 'Match record''s human-readable unique identifier.';
COMMENT ON COLUMN matches.created_at IS 'Match record''s creation date/time.';
COMMENT ON COLUMN matches.initator IS 'Match record''s initiating entity.';
COMMENT ON COLUMN matches.hash IS 'Value of hash used to identify match.';
COMMENT ON COLUMN matches.hash_type IS 'Type of hash used to identify match.'
COMMENT ON COLUMN matches.input IS 'Incoming data from real-time match request.';
COMMENT ON COLUMN matches.data IS 'Response data from match request.';
COMMENT ON COLUMN matches.invalid IS 'Indicator used for designating match as invalid.';
COMMENT ON COLUMN matches.status IS 'Match record''s status.';

COMMIT;
