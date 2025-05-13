CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

DROP TABLE IF EXISTS cash_postings;

CREATE TABLE cash_postings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
    amount NUMERIC(18,2) NOT NULL,
    posting_type CHAR(1) NOT NULL,
    description VARCHAR(255) NULL
);

--CREATE INDEX ix_cash_postings_created_at ON cash_postings (created_at);
CREATE INDEX ix_cash_postings_created_at_date ON cash_postings (DATE(created_at));