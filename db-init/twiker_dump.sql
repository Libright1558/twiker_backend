--
-- PostgreSQL database dump
--

-- Dumped from database version 16.4 (Postgres.app)
-- Dumped by pg_dump version 16.4 (Postgres.app)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Create database; Name: twiker; Owner: postgres
--

CREATE DATABASE twiker;

\c twiker;

--
-- Name: uuid-ossp; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS "uuid-ossp" WITH SCHEMA public;


--
-- Name: EXTENSION "uuid-ossp"; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION "uuid-ossp" IS 'generate universally unique identifiers (UUIDs)';


SET default_tablespace = '';

SET default_table_access_method = heap;


--
-- Name: like_table; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE IF NOT EXISTS public.like_table (
    "postId" uuid NOT NULL,
    username character varying(50) NOT NULL,
    "createdAt" timestamp with time zone DEFAULT now()
);


ALTER TABLE public.like_table OWNER TO "postgres";

--
-- Name: pinned_table; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE IF NOT EXISTS public.pinned_table (
    "postId" uuid NOT NULL
);


ALTER TABLE public.pinned_table OWNER TO "postgres";

--
-- Name: post_table; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE IF NOT EXISTS public.post_table (
    "postId" uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    postby character varying(50) NOT NULL,
    content text,
    "createdAt" timestamp with time zone DEFAULT now()
);


ALTER TABLE public.post_table OWNER TO "postgres";

--
-- Name: retweet_table; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE IF NOT EXISTS public.retweet_table (
    "postId" uuid NOT NULL,
    username character varying(50) NOT NULL,
    "createdAt" timestamp with time zone DEFAULT now()
);


ALTER TABLE public.retweet_table OWNER TO "postgres";

--
-- Name: user_table; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE IF NOT EXISTS public.user_table (
    "userId" uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    firstname character varying(50),
    lastname character varying(50),
    username character varying(50) NOT NULL,
    email character varying(50),
    password text,
    profilepic text,
    "createdAt" timestamp with time zone DEFAULT now(),
    "updatedAt" timestamp with time zone DEFAULT now()
);


ALTER TABLE public.user_table OWNER TO "postgres";


--
-- Name: like_table like_table_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.like_table
    ADD CONSTRAINT like_table_pkey PRIMARY KEY ("postId");


--
-- Name: pinned_table pinned_table_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pinned_table
    ADD CONSTRAINT pinned_table_pkey PRIMARY KEY ("postId");


--
-- Name: post_table post_table_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.post_table
    ADD CONSTRAINT post_table_pkey PRIMARY KEY ("postId");


--
-- Name: retweet_table retweet_table_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.retweet_table
    ADD CONSTRAINT retweet_table_pkey PRIMARY KEY ("postId");


--
-- Name: user_table user_table_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_table
    ADD CONSTRAINT user_table_pkey PRIMARY KEY ("userId");


--
-- Name: user_table user_table_username_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_table
    ADD CONSTRAINT user_table_username_key UNIQUE (username);


--
-- Name: user_table user_table_username_key1; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_table
    ADD CONSTRAINT user_table_username_key1 UNIQUE (username);


--
-- Name: user_table user_table_username_key2; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_table
    ADD CONSTRAINT user_table_username_key2 UNIQUE (username);


--
-- Name: user_table user_table_username_key3; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_table
    ADD CONSTRAINT user_table_username_key3 UNIQUE (username);


--
-- Name: like_table like_table_postId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.like_table
    ADD CONSTRAINT "like_table_postId_fkey" FOREIGN KEY ("postId") REFERENCES public.post_table("postId") ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: like_table like_table_username_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.like_table
    ADD CONSTRAINT like_table_username_fkey FOREIGN KEY (username) REFERENCES public.user_table(username) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: pinned_table pinned_table_postId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pinned_table
    ADD CONSTRAINT "pinned_table_postId_fkey" FOREIGN KEY ("postId") REFERENCES public.post_table("postId") ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: post_table post_table_postby_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.post_table
    ADD CONSTRAINT post_table_postby_fkey FOREIGN KEY (postby) REFERENCES public.user_table(username) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: retweet_table retweet_table_postId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.retweet_table
    ADD CONSTRAINT "retweet_table_postId_fkey" FOREIGN KEY ("postId") REFERENCES public.post_table("postId") ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: retweet_table retweet_table_username_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.retweet_table
    ADD CONSTRAINT retweet_table_username_fkey FOREIGN KEY (username) REFERENCES public.user_table(username) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Create user; Name: normal;
--

CREATE ROLE normal WITH LOGIN PASSWORD 'test';
GRANT SELECT, INSERT, UPDATE, DELETE ON user_table, pinned_table, post_table, retweet_table, like_table TO normal;

--
-- PostgreSQL database dump complete
--

