$full = $args[0]

# ����·�� 
# ��ȡ��Ŀ¼����·��
$rootFolder = (Get-Item -Path "./" -Verbose).FullName

# ����ģʽ�µĽ�������б���Ҫ���������Ŀ�Ͳ���������Ŀ��.sln�ļ�����·��
$solutionPaths = @(
		"../"
	)

if ($full -eq "-f")
{
	# ����������Ҫ���������������Ʃ��WPF��Ŀ
	$solutionPaths += (
		"../"
	) 
}else{ 
	Write-host ""
	Write-host ":::::::::::::: !!! You are in development mode !!! ::::::::::::::" -ForegroundColor red -BackgroundColor  yellow
	Write-host "" 
} 
