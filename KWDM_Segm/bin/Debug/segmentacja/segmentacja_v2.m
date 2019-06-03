close all;
clc;
clear;
img = imread('ld.png');
im_gr = rgb2gray(img); % na greyscale
figure 
imshow(im_gr);
im_grn = im_gr;
dim = size(im_gr); %rozmiary obrazu

img_gr = medfilt2(im_gr);
figure 
imshow(img_gr)
%im_gr = imdiffusefilt(img_gr,'NumberOfIterations',10);

figure 
imshow(im_gr);
title('Po preprocessingu');

win_size1 = floor((dim(1)/40)); %podzielnoœæ na 20
win_size2 = floor((dim(2)/40));

crop1 = win_size1*40; %ile pikseli mo¿na zostawiæ w obrazie, aby okna idealnie pasowa³y
crop2 = win_size2*40;

im_cropped = im_gr(1:crop1,1:crop2); %przycinanie obrazu
dim_crop = size(im_cropped); %rozmiary przyciêtego

win_means = zeros(win_size1,win_size2); %macierz œrednich intensywnoœci dla okien
k=1;
l=1;

%% okienkowanie + œrednia

for i = 1:10:dim_crop(1)  %od 1 co 20 pikseli (rozmiar okna) do koñca obrazu
    for j = 1:10:dim_crop(2)
        win = im_gr(i:i+9,j:j+9); %definicja okna
        wmean = mean2(win); %œrednia w oknie
        win_means(k,l) = wmean; %zapis œrednich do macierzy
        l=l+1;
    end
    k=k+1;
    l=1;
end

%% maksima lokalne 
win_xcount = size(win_means,1);
max_loc = cell(win_xcount,1);

for i = 1:win_xcount
    means_row = win_means(i,:); %szukanie w osi x
    [pks,locs] = findpeaks(means_row); %lokalne maksima
    pks_loc = locs(find(pks>100)); %tylko indeksy z maksimum jasnoœci piksela > 100
    max_loc{i} = pks_loc; 
end

%% cell na pojedynczy wektor, wyszukiwanie okien z krawêdziami
[size1 size2] = size(max_loc);
cells_size = 0;

for j = 1:size1
    if (j == 1)
        concat = max_loc{j};
    else
        concat = [concat max_loc{j}]; %cell na wektor
    end  
end

% [uniques,numUnique] = count_unique(concat); %okna z maksimami
% uniques = uniques';

%% oddzielanie membrany od naskórka + pozycja naskórka do maski
%opcja : wyci¹gn¹c z max_loca index okna >18 (zmienna index) dla ka¿dego z
%wektorów, potem pomno¿yæ *20 index w x,y oraz ustawiæ jako punkty do
%startowego konturu
mask=zeros(size(max_loc,1),2);
delta = 0;
for n = 1:(size(max_loc,1))
    penez = cell2mat(max_loc(n));
    for o = 1:(size(penez,2)-1) %-1, bo wykonanych ró¿nic zawsze bêdzie n-1
        delta = delta + (penez(o+1) - penez(o)); %suma ró¿nic miêdzy indeksami okien
    end
    if(o>=1)
        epsilon = delta/o;
    elseif(o == [])
        mask(n,1) = 0;
        mask(n,2) = 0;
    elseif(o == 0)
        position = penez; %zapisanie pozycji okna z przeskokiem indeksu wiêkszym od epsilon
        mask(n,1) = position-5;
        mask(n,2) = position+5;
    else
        mask(n,1) = 0;
        mask(n,2) = 0;
    end
    for p = 1:(size(penez,2)-1)
        delta = penez(p+1) - penez(p);
        if (delta >= epsilon) 
            position = penez(p+1)*10; %zapisanie pozycji okna z przeskokiem indeksu wiêkszym od epsilon
            mask(n,1) = position-10;
            mask(n,2) = position+10;
            break
        end
    end
    delta = 0;
%     mask(n,1) = max_loc(n)-5;
%     mask(n,2) = max_loc(n)+5;
end
co = zeros(crop1,crop2);
for r = 1:(size(max_loc,1))
    if(r == 1)
    co(1:10,mask(r,1):mask(r,2))=1;
    else
        if(mask(r,1) ~= 0 && mask(r,2) ~= 0)
        co(r*10-10:r*10,mask(r,1):mask(r,2))=1; %pozycja okna z pikiem *10 (wielkoœæ okna)
        else
        co(r*10-10:r*10,crop2)=1; %w razie gdyby nie wykry³o maximów, maska przyjmie dla tych okien 1 tylko w ostatniej kolumnie
        end
    end
end

figure; imshow(co,[]); title('Initial Contour Location');
%% aktywne kontury

bw = activecontour(im_cropped,co,600,'Chan-Vese','SmoothFactor',0.1,'ContractionBias',0.3); %wczesniej - 0.1 0.5 400
figure
imshow(bw)
title('Segmented Image - Chan-Vese')

bw2 = activecontour(im_cropped,co,120,'edge','SmoothFactor',0.1,'ContractionBias',0.8); 
figure
imshow(bw2)
title('Segmented Image - edge')

figure
subplot(2,1,1)
imshow(bw2);
title('Segmented Image - edge')
subplot(2,1,2)
imshow(im_gr);
title('Input image')

figure
subplot(2,1,1)
imshow(bw);
title('Segmented Image - Chan-Vese')
subplot(2,1,2)
imshow(im_gr);
title('Input image')

%% druga segmentacja, bo czemu nie
% bw = activecontour(im_cropped,bw,400,'Chan-Vese','SmoothFactor',0.1,'ContractionBias',0.5);
% figure
% imshow(bw)
% title('Segmented Image - Chan-Vese v2')
% 
% SE = strel('cube',3)
% 
% bw2 = imclose(bw,SE);
% figure
% imshow(bw2)