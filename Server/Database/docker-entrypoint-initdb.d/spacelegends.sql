CREATE TABLE `users` (
  `id` integer PRIMARY KEY AUTO_INCREMENT,
  `username` varchar(16) UNIQUE NOT NULL,
  `email` varchar(255) UNIQUE NOT NULL,
  `email_verified` bool NOT NULL DEFAULT false COMMENT 'If the user has proven that this is his email',
  `email_verification_code` integer NOT NULL COMMENT 'The code that the user must verify',
  `display_name` varchar(24),
  `password` char(128) NOT NULL,
  `salt` char(32) NOT NULL,
  `wallet_address` varchar(42) NOT NULL,
  `wallet_verified` bool NOT NULL DEFAULT false COMMENT 'If the user has proven that this is his wallet'
);

CREATE TABLE `nfts` (
  `id` integer PRIMARY KEY,
  `type` integer,
  `user_id` integer,
  `is_minted` bool
);

ALTER TABLE `nfts` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`id`);
