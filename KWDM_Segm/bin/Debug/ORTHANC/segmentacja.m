function[] = segmentacja(file_path, im_path)

    img = dicomread(strcat(file_path, im_path));
    if (size(img,3) > 1 || size(img,4) > 1)
        im_gr = rgb2gray(img); % na greyscale
    else
        im_gr = img;
    end
    dim = size(im_gr); %rozmiary obrazu

    win_size1 = floor((dim(1)/40)); %podzielno�� na 20
    win_size2 = floor((dim(2)/40));

    crop1 = win_size1*40; %ile pikseli mo�na zostawi� w obrazie, aby okna idealnie pasowa�y
    crop2 = win_size2*40;

    im_cropped = im_gr(1:crop1,1:crop2); %przycinanie obrazu
    dim_crop = size(im_cropped); %rozmiary przyci�tego

    win_means = zeros(win_size1,win_size2); %macierz �rednich intensywno�ci dla okien
    k=1;
    l=1;

    %% okienkowanie + �rednia

    for i = 1:10:dim_crop(1)  %od 1 co 20 pikseli (rozmiar okna) do ko�ca obrazu
        for j = 1:10:dim_crop(2)
            win = im_gr(i:i+9,j:j+9); %definicja okna
            wmean = mean2(win); %�rednia w oknie
            win_means(k,l) = wmean; %zapis �rednich do macierzy
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
        pks_loc = locs(find(pks>100)); %tylko indeksy z maksimum jasno�ci piksela > 100
        max_loc{i} = pks_loc; 
    end

    %% cell na pojedynczy wektor, wyszukiwanie okien z kraw�dziami
    
    [size1 size2] = size(max_loc);
    cells_size = 0;

    for j = 1:size1
        if (j == 1)
            concat = max_loc{j};
        else
            concat = [concat max_loc{j}]; %cell na wektor
        end  
    end

    %% oddzielanie membrany od nask�rka + pozycja nask�rka do maski
    
    %opcja : wyci�gn�c z max_loca index okna >18 (zmienna index) dla ka�dego z
    %wektor�w, potem pomno�y� *20 index w x,y oraz ustawi� jako punkty do
    %startowego konturu
    mask=zeros(size(max_loc,1),2);
    delta = 0;
    for n = 1:(size(max_loc,1))
        penez = cell2mat(max_loc(n));
        for o = 1:(size(penez,2)-1) %-1, bo wykonanych r�nic zawsze b�dzie n-1
            delta = delta + (penez(o+1) - penez(o)); %suma r�nic mi�dzy indeksami okien
        end
        if(o>=1)
            epsilon = delta/o;
        elseif(o == [])
            mask(n,1) = 0;
            mask(n,2) = 0;
        elseif(o == 0)
            position = penez; %zapisanie pozycji okna z przeskokiem indeksu wi�kszym od epsilon
            mask(n,1) = position-5;
            mask(n,2) = position+5;
        else
            mask(n,1) = 0;
            mask(n,2) = 0;
        end
        for p = 1:(size(penez,2)-1)
            delta = penez(p+1) - penez(p);
            if (delta >= epsilon) 
                position = penez(p+1)*10; %zapisanie pozycji okna z przeskokiem indeksu wi�kszym od epsilon
                mask(n,1) = position-10;
                mask(n,2) = position+10;
                break
            end
        end
        delta = 0;
    end
    co = zeros(crop1,crop2);
    for r = 1:(size(max_loc,1))
        if(r == 1)
            if(mask(r,1) == 0)
                mask(r,1)= 1;
            end
            if(mask(r,2) == 0)
                mask(r,2) = 1;
            end
            
        co(1:10,mask(r,1):mask(r,2))=1;
        else
            if(mask(r,1) ~= 0 && mask(r,2) ~= 0)
            co(r*10-10:r*10,mask(r,1):mask(r,2))=1; %pozycja okna z pikiem *10 (wielko�� okna)
            else
            co(r*10-10:r*10,crop2)=1; %w razie gdyby nie wykry�o maxim�w, maska przyjmie dla tych okien 1 tylko w ostatniej kolumnie
            end
        end
    end
    
    %% aktywne kontury

    bw = activecontour(im_cropped,co,600,'Chan-Vese','SmoothFactor',0.1,'ContractionBias',0.3); %wczesniej - 0.1 0.5 400

    img_cropped = img(1:crop1,1:crop2);
    mask = uint8(bw);
    
    %% histogram
    
    [rows, columns] = size(co);
    deltaX = 20; % DO USTAWIENIA - PRZESUNI�CIE X
    deltaY = 0; % DO USTAWIENIA - PRZESUNI�CIE Y
    D = zeros(rows, columns, 2);
    D(:,:,1) = -deltaX; 
    D(:,:,2) = -deltaY;
    warpedImage = imwarp(co, D);

    %% obwiednia
    
    im_mask = mask.*im_cropped;
    im_mask2= im_mask(im_mask~=0);

    f = figure('visible', 'off');
    xlabel = 'Intensywno�� pikseli';
    ylabel = 'Ilo�� pikseli';
    title = 'Histogram obrazu maski';
    h2=histfit(double(im_mask2),50,'nakagami');
     
    %% zapis
     
    saveas(f, fullfile(file_path, 'Histogram.png'), 'png');
    imwrite(bw, strcat(file_path, '\segmCV', '.png'), 'png');
    
end