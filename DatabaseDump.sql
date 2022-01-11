-- phpMyAdmin SQL Dump
-- version 4.9.4
-- https://www.phpmyadmin.net/
--
-- Host: localhost
-- Generation Time: Jan 11, 2022 at 02:02 PM
-- Server version: 10.2.32-MariaDB
-- PHP Version: 5.5.14

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `dennisborst`
--

-- --------------------------------------------------------

--
-- Table structure for table `gamesPlayed`
--

CREATE TABLE `gamesPlayed` (
  `id` int(11) NOT NULL,
  `date` date NOT NULL DEFAULT current_timestamp(),
  `playerOne` int(10) UNSIGNED NOT NULL,
  `playerTwo` int(10) UNSIGNED NOT NULL,
  `winningPlayer` int(10) UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `gamesPlayed`
--

INSERT INTO `gamesPlayed` (`id`, `date`, `playerOne`, `playerTwo`, `winningPlayer`) VALUES
(1, '2021-12-28', 2, 1, 1),
(2, '2021-12-28', 2, 3, 3),
(3, '2021-12-28', 2, 3, 3),
(4, '2021-12-28', 2, 3, 3),
(5, '2021-12-28', 2, 3, 2),
(6, '2021-12-28', 2, 3, 2),
(7, '2021-12-28', 2, 3, 3),
(8, '2021-12-28', 3, 1, 3),
(9, '2021-12-28', 1, 2, 1),
(10, '2021-12-29', 1, 3, 1),
(11, '2021-12-29', 1, 3, 1),
(12, '2021-12-29', 1, 3, 1),
(13, '2021-12-29', 1, 3, 3),
(14, '2022-01-04', 3, 1, 3),
(15, '2022-01-07', 1, 3, 1),
(16, '2022-01-07', 1, 3, 3),
(17, '2022-01-07', 3, 1, 1),
(18, '2022-01-07', 3, 1, 1),
(19, '2021-12-11', 2, 1, 1),
(20, '2022-01-11', 5, 4, 5),
(21, '2022-01-11', 4, 5, 4),
(22, '2022-01-11', 4, 6, 4),
(23, '2022-01-11', 4, 6, 6),
(24, '2022-01-11', 4, 6, 6),
(25, '2022-01-11', 4, 6, 4),
(26, '2022-01-11', 4, 6, 4),
(27, '2022-01-11', 4, 6, 4),
(28, '2022-01-11', 4, 6, 4),
(29, '2022-01-11', 4, 6, 4),
(30, '2022-01-11', 4, 6, 4),
(31, '2022-01-10', 4, 6, 4),
(32, '2022-01-10', 4, 6, 4),
(33, '2022-01-11', 6, 1, 6),
(34, '2022-01-11', 8, 1, 8),
(35, '2022-01-11', 9, 8, 9);

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `username` tinytext CHARACTER SET utf8 DEFAULT NULL,
  `password` tinytext CHARACTER SET utf8 DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`id`, `username`, `password`) VALUES
(1, 'DenTheMan', 'Cool123'),
(2, 'hfuiewgui', 'gwegwe'),
(3, 'woef', 'blabla'),
(4, 'Markreel', 'KrijgJeTochNiet123'),
(5, 'Henk', 'Klaass'),
(6, 'Pieter', 'Kaas123'),
(7, 'CooleKikker', 'Swag123'),
(8, 'Aaron', 'Networking'),
(9, 'Tonnie', 'PHPMaster');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `gamesPlayed`
--
ALTER TABLE `gamesPlayed`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `gamesPlayed`
--
ALTER TABLE `gamesPlayed`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=36;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
