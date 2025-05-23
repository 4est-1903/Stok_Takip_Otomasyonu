using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace StokTakipOtomasyonu
{
    public partial class frmMusteriListele : Form
    {
        public frmMusteriListele()
        {
            InitializeComponent();
        }

        MySqlConnection baglan = new MySqlConnection("Server=localhost;Database=stok_takip;Uid=root;Pwd=root;");
        DataSet ds = new DataSet();

        private void frmMusteriListele_Load(object sender, EventArgs e)
        {
            KayitGoster();
        }

        private void KayitGoster()
        {
            try
            {
                // Bağlantıyı aç
                baglan.Open();

                // Veritabanından tüm müşteri bilgilerini çek
                MySqlDataAdapter adtr = new MySqlDataAdapter("SELECT * FROM musteriler", baglan);
                DataTable dt = new DataTable();
                adtr.Fill(dt); // DataTable'a veri doldur

                // DataGridView'e veri at
                dataGridView1.DataSource = dt;

                // Eğer veri yoksa kullanıcıyı uyar
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tabloda hiç veri bulunamadı.");
                }

                // Bağlantıyı kapat
                baglan.Close();
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya mesaj göster
                MessageBox.Show("Veri çekme hatası: " + ex.Message);
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            txttckimlik.Text = dataGridView1.CurrentRow.Cells["tc"].Value.ToString();
            txtadsoyad.Text = dataGridView1.CurrentRow.Cells["adsoyad"].Value.ToString();
            txttelefon.Text = dataGridView1.CurrentRow.Cells["telefon"].Value.ToString();
            txtadres.Text = dataGridView1.CurrentRow.Cells["adres"].Value.ToString();
            txteposta.Text = dataGridView1.CurrentRow.Cells["email"].Value.ToString();
        }



        private void txtTCara_TextChanged(object sender, EventArgs e)
        {

        }

        private void Temizle()
        {
            // Formdaki tüm TextBox'ları temizle
            foreach (Control item in this.Controls)
            {
                if (item is TextBox)
                    item.Text = "";
            }
        }

        private void btnguncelle_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Veritabanına bağlantıyı aç
                baglan.Open();

                // Güncelleme sorgusunu hazırla
                MySqlCommand komut = new MySqlCommand(
                    "UPDATE musteriler SET adsoyad=@adsoyad, telefon=@telefon, adres=@adres, email=@email WHERE tc=@tc", baglan);
                komut.Parameters.AddWithValue("@tc", txttckimlik.Text);
                komut.Parameters.AddWithValue("@adsoyad", txtadsoyad.Text);
                komut.Parameters.AddWithValue("@telefon", txttelefon.Text);
                komut.Parameters.AddWithValue("@adres", txtadres.Text);
                komut.Parameters.AddWithValue("@email", txteposta.Text);

                // Sorguyu çalıştır
                komut.ExecuteNonQuery();

                // Bağlantıyı kapat
                baglan.Close();

                // Kullanıcıyı bilgilendir
                MessageBox.Show("Müşteri bilgileri güncellendi.");

                // Tablodaki verileri yeniden yükle
                KayitGoster();

                // Formu temizle
                Temizle();
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya mesaj göster
                baglan.Close();
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void btnsil_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Seçilen kaydın TC'sini al
                string tc = dataGridView1.CurrentRow.Cells["tc"].Value.ToString();

                // Veritabanına bağlantıyı aç
                baglan.Open();

                // Silme sorgusunu hazırla
                MySqlCommand komut = new MySqlCommand("DELETE FROM musteriler WHERE tc=@tc", baglan);
                komut.Parameters.AddWithValue("@tc", tc);

                // Sorguyu çalıştır
                komut.ExecuteNonQuery();

                // Bağlantıyı kapat
                baglan.Close();

                // Kullanıcıyı bilgilendir
                MessageBox.Show("Müşteri kaydı silindi.");

                // Tablodaki verileri yeniden yükle
                KayitGoster();
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya mesaj göster
                baglan.Close();
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void frmMusteriListele_DoubleClick(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellDoubleClick_1(object sender, DataGridViewCellEventArgs e)
        {
            // DataGridView'den seçilen müşteri bilgilerini formda göstermek için
            txttckimlik.Text = dataGridView1.CurrentRow.Cells["tc"].Value.ToString();
            txtadsoyad.Text = dataGridView1.CurrentRow.Cells["adsoyad"].Value.ToString();
            txttelefon.Text = dataGridView1.CurrentRow.Cells["telefon"].Value.ToString();
            txtadres.Text = dataGridView1.CurrentRow.Cells["adres"].Value.ToString();
            txteposta.Text = dataGridView1.CurrentRow.Cells["email"].Value.ToString();
        }

        private void btnTcAra_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTCara.Text))
                {
                    KayitGoster(); // Tüm kayıtları gösteren fonksiyonun
                    return;
                }

                DataTable tablo = new DataTable();
                using (MySqlConnection baglanti = new MySqlConnection("server=localhost;user id=root;password= root;database=stok_takip"))
                {
                    baglanti.Open();
                    string sorgu = "SELECT * FROM musteriler WHERE tc LIKE @tc COLLATE utf8mb4_general_ci";
                    MySqlDataAdapter adtr = new MySqlDataAdapter(sorgu, baglanti);
                    adtr.SelectCommand.Parameters.AddWithValue("@tc", "%" + txtTCara.Text + "%");
                    adtr.Fill(tablo);
                }

                dataGridView1.DataSource = tablo;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
    }
}
