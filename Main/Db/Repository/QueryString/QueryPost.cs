namespace Db.QueryPostSql;

public static class QueryPostSqlString
{
    /*
    * fetch Post data
    */
    public const string FetchPost = @"
    WITH 
    likeNum AS (
        SELECT post_table.""postId"", COUNT(like_table.username) AS ""likeNum""
        FROM post_table
        LEFT JOIN like_table ON post_table.""postId"" = like_table.""postId""
        WHERE post_table.postby = @PostBy
        GROUP BY post_table.""postId""
    ),
    retweetNum AS (
        SELECT post_table.""postId"", COUNT(retweet_table.username) AS ""retweetNum""
        FROM post_table 
        LEFT JOIN retweet_table ON post_table.""postId"" = retweet_table.""postId""
        WHERE post_table.postby = @PostBy
        GROUP BY post_table.""postId""
    ),
    selfLike AS (
        SELECT post_table.""postId"",
        CASE
            WHEN like_table.username IS NULL THEN 0
            ELSE 1
        END AS ""selfLike""
        FROM post_table
        LEFT JOIN like_table ON post_table.""postId"" = like_table.""postId"" AND post_table.postby = like_table.username
        WHERE post_table.postby = @PostBy
    ),
    selfRetweet AS (
        SELECT post_table.""postId"", 
        CASE
            WHEN retweet_table.username IS NULL THEN 0
            ELSE 1
        END AS ""selfRetweet""
        FROM post_table
        LEFT JOIN retweet_table ON post_table.""postId"" = retweet_table.""postId"" AND post_table.postby = retweet_table.username
        WHERE post_table.postby = @PostBy
    ),
    userInfo AS (
        SELECT post_table.""postId"", user_table.firstname, user_table.lastname, user_table.profilepic
        FROM post_table 
        INNER JOIN user_table ON post_table.postby = user_table.username
        WHERE post_table.postby = @PostBy
    )
    SELECT 
        p.postby, 
        p.content, 
        p.""createdAt"", 
        p.""postId"", 
        ln.""likeNum"", 
        rn.""retweetNum"", 
        sr.""selfRetweet"", 
        sl.""selfLike"",
        ui.firstname, 
        ui.lastname, 
        ui.profilepic
    FROM post_table p
    LEFT JOIN likeNum ln ON p.""postId"" = ln.""postId""
    LEFT JOIN retweetNum rn ON p.""postId"" = rn.""postId""
    LEFT JOIN selfLike sl ON p.""postId"" = sl.""postId""
    LEFT JOIN selfRetweet sr ON p.""postId"" = sr.""postId""
    LEFT JOIN userInfo ui ON p.""postId"" = ui.""postId""
    WHERE p.postby = @PostBy
    ORDER BY p.""createdAt"" DESC";
}