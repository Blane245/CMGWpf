DROP TABLE IF EXISTS `notesequence`;
CREATE TABLE `notesequence` (
  `name` varchar(45) NOT NULL,
  `items` longtext NOT NULL,
  PRIMARY KEY (`name`)
);
INSERT INTO `notesequence` (`name`, `items`) VALUES (
('ascending c scale',
'[{\"id\":\"e4bba982-a701-50b5-8640-e6afcc95bef4\",\"value\":60,\"beats\":1}]')
);