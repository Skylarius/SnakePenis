<?php
 
 require ('../snakepenis/mysqli-connect-snakepenis.php');
 
$query = "SELECT * FROM `SnakePenisScore` ORDER by `SCORE` DESC LIMIT 10";
$result = mysqli_query($dbc, $query) or die('Query failed: ' . mysqli_error($dbc));
 
$num_results = mysqli_num_rows($result);
 
for($i = 0; $i < $num_results; $i++)
{
     $row = mysqli_fetch_array($result);
     echo $row['ID'] . "\t" . $row['NAME'] . "\t" . $row['LENGTH'] . "\t" . $row['SCORE'] . "\n";
}
?>