DROP TABLE IF EXISTS `notesequence_tag`;
CREATE TABLE `notesequence_tag` (
  `notesequence_name` varchar(45) DEFAULT NULL,
  `tag_name` varchar(45) DEFAULT NULL,
  KEY `sequence_idx` (`notesequence_name`),
  KEY `tag_name_idx` (`tag_name`),
  CONSTRAINT `notesequence_tag_ibfk_1` FOREIGN KEY (`notesequence_name`) REFERENCES `notesequence` (`name`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `notesequence_tag_ibfk_2` FOREIGN KEY (`tag_name`) REFERENCES `tag` (`name`) ON DELETE CASCADE ON UPDATE CASCADE
);
INSERT INTO `notesequence_tag` (`notesequence_name`, `tag_name`) VALUES 
('notenametest','test'),
('BicycleBuiltForTwo1', 'rhyme'),
('BicycleBuiltForTwo1R', 'rhyme'),
('BicycleBuiltForTwo4R', 'rhyme'),
('BicycleBuiltForTwo3R', 'rhyme'),
('BicycleBuiltForTwo2R', 'rhyme'),
('BicycleBuiltForTwo0', 'rhyme'),
('BicycleBuiltForTwo2', 'rhyme'),
('BicycleBuiltForTwo3', 'rhyme'),
('BicycleBuiltForTwo4', 'rhyme'),
('ScaleEtude', 'Etude'),
('FullScale', 'Etude')
;
