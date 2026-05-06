$names = @('success','error','interact','startup','exit')
$freqs = @(880,440,660,523,392)
$w = 22050
$dur = 0.3
$ns = [int]($w * $dur)
for ($n = 0; $n -lt $names.Length; $n++) {
    $name = $names[$n]
    $freq = $freqs[$n]
    $data = [System.Collections.Generic.List[byte]]::new()
    for ($i = 0; $i -lt $ns; $i++) {
        $sign = if (([int]($i * $freq / $w) % 2) -eq 0) { 1 } else { -1 }
        $val = [int](16000 * $sign * [Math]::Max(0, 1 - $i / $ns))
        $val = [Math]::Max(-32768, [Math]::Min(32767, $val))
        $bytes = [System.BitConverter]::GetBytes([int16]$val)
        $data.AddRange($bytes)
    }
    $path = "D:\DailyMeal\DailyMeal\Sound\$name.wav"
    $chunkSize = 36 + $data.Count
    $header = [System.Collections.Generic.List[byte]]::new()
    $header.AddRange([System.Text.Encoding]::ASCII.GetBytes('RIFF'))
    $header.AddRange([System.BitConverter]::GetBytes([int32]$chunkSize))
    $header.AddRange([System.Text.Encoding]::ASCII.GetBytes('WAVEfmt '))
    $header.AddRange([System.BitConverter]::GetBytes([int32]16))
    $header.AddRange([System.BitConverter]::GetBytes([int16]1))
    $header.AddRange([System.BitConverter]::GetBytes([int16]1))
    $header.AddRange([System.BitConverter]::GetBytes([int32]$w))
    $header.AddRange([System.BitConverter]::GetBytes([int32]($w*2)))
    $header.AddRange([System.BitConverter]::GetBytes([int16]2))
    $header.AddRange([System.BitConverter]::GetBytes([int16]16))
    $header.AddRange([System.Text.Encoding]::ASCII.GetBytes('data'))
    $header.AddRange([System.BitConverter]::GetBytes([int32]$data.Count))
    $all = [System.Collections.Generic.List[byte]]::new()
    $all.AddRange($header)
    $all.AddRange($data)
    [System.IO.File]::WriteAllBytes($path, [byte[]]$all)
    Write-Output "Created: $name.wav ($($all.Count) bytes)"
}
