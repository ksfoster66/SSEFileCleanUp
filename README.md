Using a supplied exclusion lsit file, in either a .txt or .csv format, removes all excluded files from provided source meshes directory and textures directory into an Excluded folder.

Command line flags

m - meshes directory

t - textures directory

l - exclusion list

o - output directory

e - meshes file extensions, defaults to nif and tri. When supplying extensions, seperate with a comma and leave out the dot

i - textures file extensions, defaults to dds and tga. When supplying extensions, seperate with a comma and leave out the dot

k - keep files. Normally program removes files from their original location, provide this flag to keep them in their original location.

Outputs:

Default output of moved files is to an Excluded directory in the location this program is run. It is best recommended to remove the directory before running again.

A list of all excluded files

A list of all kept files

Inside the excluded directory there is a list of all error files, either that they already existed upon copying(Possibly due to being supplied multiple times in the exclusion list), or because the file didn't exist.
