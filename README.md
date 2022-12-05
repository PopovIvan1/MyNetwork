WEB PORTAL FOR REVIEWS "WHAT SHOOULD U READ/WATCH/PLAY, etc."

C#/ASP.NET Core MVC/Entity Framework/SQL Server/MySQL/PostgreSQL, Bootstrap

Non-authenticated users have read-only access (they can use search, but can’t create reviews, can’t leave comments, rating and likes).

Authenticated not-admins have access to everything except admin-page. In basic implementation admin page provides only user list (links to user pages).

It's necessary to suppot authentication via social networks (at least two).

Admin see every user page and review as its creates (e.g. admin can edit review or creates review under user from his/her page).

Every page provides access to full-text search over whole app (results are represented as a review list, e.g., if some text is found in comment, the results page diplays link to the corresponding review page).

Every user has a personal page, which contains table of the reviews (table should support filters, sorting, actions for review creation/deletion/editing/opening in a read mode).

Every review has: review name, name of the reviewed piece of art, "group" (from the fixed set: "Movies", "Books", "Games" и т.п.), tags (multiple tags with autocomplition - when users starts entering tag, dropdown show variants, which already exist in the app), review text обзора (with "markdown" formatting), optional image (stored in the cloud) and the grade in the range from 0 to 10.

All images are stored in the cloud, upload control should support drag-n-drop.

On the main page: recently added reviews, reviews with the highest grades, tag cloud.