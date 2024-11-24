#!/bin/bash


# Function to extract token
extract_token() {
    grep -i "authorization:" | awk '{print $3}'
}

# Register
curl -s \
    -X POST \
    http://localhost:80/register \
    -H "Content-Type: application/json" \
    -H 'accept: */*' \
    -d '{"FirstName": "who", "LastName": "why", "Username": "noBody", "Email": "noBody@email.com", "Password": "unknown"}'


# Login
Refresh_token=$(curl -s \
    -D - \
    -X POST \
    http://localhost:80/login \
    -H "Content-Type: application/json" \
    -H 'accept: */*' \
    -d '{"Username": "noBody", "Password": "unknown"}' | \
    extract_token)


# RefreshToken
Access_token=$(curl \
    -D - \
    -X POST \
    http://localhost:80/refresh  \
    -H "Authorization: Bearer ${Refresh_token}" \
    -H 'accept: */*' \
    -d '' | extract_token)


# CreatePost
curl -X POST \
    http://localhost:80/createPost  \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer ${Access_token}" \
    -H 'accept: */*' \
    -d '"today is a good day."'


# GetPosts
PostId=$(curl -o - \
    -X GET \
    http://localhost:80/getPost  \
    -H "Authorization: Bearer ${Access_token}" \
    -H 'accept: */*' | awk -F '"' '/"postId"/{print $4}')


# UpdateLike
curl -X PUT \
    http://localhost:80/like \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer ${Access_token}" \
    -H 'accept: */*' \
    -d "\"${PostId}\""


# UpdateRetweet
curl -X PUT \
    http://localhost:80/retweet  \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer ${Access_token}" \
    -H 'accept: */*' \
    -d "\"${PostId}\""


# DeletePost
curl -X DELETE \
    http://localhost:80/deletePost  \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer ${Access_token}" \
    -H 'accept: */*' \
    -d "\"${PostId}\""


# GetPersonalData
curl -X GET \
    http://localhost:80/getUser  \
    -H "Authorization: Bearer ${Access_token}" \
    -H 'accept: */*'


# DeleteAccount
curl -X DELETE \
    http://localhost:80/deleteAccount  \
    -H "Content-Type: application/json" \
    -H 'accept: */*' \
    -H "Authorization: Bearer ${Access_token}" \
    -d '"noBody@email.com"'